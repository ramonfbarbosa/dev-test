using Application.Clients.Imports;
using Application.Clients.Imports.Models;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Clients.Queries.GetClientImportErrorsQuery;

public class GetClientImportErrorsQueryHandler : IRequestHandler<GetClientImportErrorsQueryRequest, GetClientImportErrorsQueryResponse>
{
    private readonly IClientControlContext _context;
    private readonly ClientImportStorageService _storageService;

    public GetClientImportErrorsQueryHandler(IClientControlContext context, ClientImportStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<GetClientImportErrorsQueryResponse> Handle(
        GetClientImportErrorsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var import = await _context.ClientImports
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("ClientImport", request.Id);

        var response = new GetClientImportErrorsQueryResponse
        {
            OriginalFileName = import.OriginalFileName,
            StatusText = GetStatusText(import.Status),
            TotalRows = import.TotalRows,
            ImportedRows = import.ImportedRows,
            FailureCount = import.FailureCount,
        };
        var summaryPath = FindSummaryFile(import.StoredFileName, import.Status);
        if (summaryPath is not null && File.Exists(summaryPath))
        {
            var json = await File.ReadAllTextAsync(summaryPath, cancellationToken);
            var summary = JsonSerializer.Deserialize<ClientImportSummary>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (summary?.Failures is not null)
            {
                response.Errors = summary.Failures
                    .Select(f => new ImportErrorDetail
                    {
                        LineNumber = f.LineNumber,
                        Message = f.Message
                    })
                    .ToList();
            }
        }
        else if (!string.IsNullOrWhiteSpace(import.ErrorMessage))
        {
            response.Errors.Add(new ImportErrorDetail
            {
                LineNumber = 0,
                Message = import.ErrorMessage
            });
        }

        return response;
    }

    private string FindSummaryFile(string storedFileName, ClientImportStatus status)
    {
        var csvFileName = Path.ChangeExtension(storedFileName, ".result.json");

        var directoryPath = status switch
        {
            ClientImportStatus.Processed => _storageService.ProcessedDirectoryPath,
            ClientImportStatus.ProcessedWithErrors => _storageService.ProcessedDirectoryPath,
            ClientImportStatus.Failed => _storageService.FailedDirectoryPath,
            _ => null
        };
        if (directoryPath is null)
        {
            return null;
        }
        return Path.Combine(directoryPath, csvFileName);
    }

    private static string GetStatusText(ClientImportStatus status) => status switch
    {
        ClientImportStatus.Pending => "Pendente",
        ClientImportStatus.Processing => "Processando",
        ClientImportStatus.Processed => "Concluído",
        ClientImportStatus.Failed => "Falhou",
        ClientImportStatus.ProcessedWithErrors => "Concluído com erros",
        _ => status.ToString()
    };
}
