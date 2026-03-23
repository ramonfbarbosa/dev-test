using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Clients.Imports.Queue;

namespace Application.Clients.Imports.Processing;

public class ClientImportBackgroundService : BackgroundService
{
    private readonly IClientImportQueue _queue;
    private readonly ClientImportStorageService _storageService;
    private readonly ClientImportProcessor _processor;
    private readonly ILogger<ClientImportBackgroundService> _logger;

    public ClientImportBackgroundService(
        IClientImportQueue queue,
        ClientImportStorageService storageService,
        ClientImportProcessor processor,
        ILogger<ClientImportBackgroundService> logger)
    {
        _queue = queue;
        _storageService = storageService;
        _processor = processor;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _storageService.EnsureDirectories();
        foreach (var filePath in _storageService.GetFilesAwaitingProcessing())
        {
            await _queue.QueueAsync(filePath, cancellationToken);
        }
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var filePath = await _queue.DequeueAsync(stoppingToken);
                await _processor.ProcessAsync(filePath, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no processamento em background da importação de clientes.");
            }
        }
    }
}
