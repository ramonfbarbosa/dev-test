using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Clients.Imports.Models;
using Application.Clients.Imports.Parsing;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Clients.Imports.Processing;

public class ClientImportProcessor
{
    private const string UnexpectedRowErrorMessage = "Erro inesperado ao processar o registro.";
    private readonly ClientImportStorageService _storageService;
    private readonly ClientImportCsvParser _csvParser;
    private readonly ClientImportRequestFactory _requestFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ClientImportProcessor> _logger;

    public ClientImportProcessor(
        ClientImportStorageService storageService,
        ClientImportCsvParser csvParser,
        ClientImportRequestFactory requestFactory,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ClientImportProcessor> logger)
    {
        _storageService = storageService;
        _csvParser = csvParser;
        _requestFactory = requestFactory;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task ProcessAsync(string filePath, CancellationToken cancellationToken)
    {
        _storageService.EnsureDirectories();
        var processingFilePath = _storageService.MoveToProcessing(filePath);
        var storedFileName = Path.GetFileName(processingFilePath);
        var summary = ClientImportSummary.Create(storedFileName);
        await UpdateImportStatusAsync(storedFileName, import => import.MarkAsProcessing(), cancellationToken);
        try
        {
            await ProcessFileAsync(processingFilePath, summary, cancellationToken);
            await CompleteSuccessfullyAsync(processingFilePath, summary, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            await HandleFatalFailureAsync(processingFilePath, summary, ex, cancellationToken);
        }
    }

    private async Task ProcessFileAsync(string filePath, ClientImportSummary summary, CancellationToken cancellationToken)
    {
        var parsedFile = _csvParser.Parse(filePath);
        foreach (var parseFailure in parsedFile.ParseFailures)
        {
            summary.RegisterParseFailure(parseFailure);

        }
        foreach (var row in parsedFile.Rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            summary.RegisterRowAttempt();
            await ProcessRowAsync(filePath, row, summary, cancellationToken);
        }
    }

    private async Task ProcessRowAsync(
        string filePath,
        ParsedClientImportRow row,
        ClientImportSummary summary,
        CancellationToken cancellationToken)
    {
        if (!_requestFactory.TryCreate(row, out var request, out var errorMessage))
        {
            summary.AddFailure(row.LineNumber, errorMessage);
            return;
        }
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(request, cancellationToken);
            summary.RegisterImportedRow();
        }
        catch (ApiValidationException validationException)
        {
            foreach (var failure in validationException.Failures)
                summary.AddFailure(row.LineNumber, $"{failure.Key}: {failure.Value}");
        }
        catch (BadRequestException badRequestException)
        {
            summary.AddFailure(row.LineNumber, badRequestException.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao importar a linha {LineNumber} do arquivo {FilePath}.", row.LineNumber, filePath);
            summary.AddFailure(row.LineNumber, UnexpectedRowErrorMessage);
        }
    }

    private async Task CompleteSuccessfullyAsync(string processingFilePath, ClientImportSummary summary, CancellationToken cancellationToken)
    {
        summary.Finish();
        var processedFilePath = _storageService.MoveToProcessed(processingFilePath);
        await _storageService.WriteSummaryAsync(processedFilePath, summary, cancellationToken);
        var storedFileName = Path.GetFileName(processedFilePath);
        await UpdateImportStatusAsync(storedFileName, import =>
            import.MarkAsProcessed(summary.TotalRows, summary.ImportedRows, summary.Failures.Count), cancellationToken);
    }

    private async Task HandleFatalFailureAsync(
        string processingFilePath,
        ClientImportSummary summary,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Erro fatal ao processar o arquivo de importação {FilePath}.", processingFilePath);
        summary.Finish();
        summary.AddFailure(0, exception.Message);
        var failedFilePath = _storageService.MoveToFailed(processingFilePath);
        await _storageService.WriteSummaryAsync(failedFilePath, summary, cancellationToken);

        var storedFileName = Path.GetFileName(failedFilePath);
        await UpdateImportStatusAsync(storedFileName, import =>
            import.MarkAsFailed(summary.TotalRows, summary.ImportedRows, summary.Failures.Count, exception.Message), cancellationToken);
    }

    private async Task UpdateImportStatusAsync(string storedFileName, Action<ClientImport> updateAction, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IClientControlContext>();
            var importRecord = await context.ClientImports
                .FirstOrDefaultAsync(x => x.StoredFileName == storedFileName, cancellationToken);

            if (importRecord is not null)
            {
                updateAction(importRecord);
                context.SetModifiedState(importRecord);
                await context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível atualizar o status da importação {StoredFileName}.", storedFileName);
        }
    }
}
