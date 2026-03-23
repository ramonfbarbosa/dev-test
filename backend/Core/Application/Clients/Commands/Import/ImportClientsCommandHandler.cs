using Application.Clients.Imports;
using Application.Clients.Imports.Queue;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Clients.Commands.Import;

public class ImportClientsCommandHandler : IRequestHandler<ImportClientsCommandRequest, ImportClientsCommandResponse>
{
    private readonly ClientImportStorageService _clientImportStorageService;
    private readonly IClientImportQueue _clientImportQueue;
    private readonly IClientControlContext _context;

    public ImportClientsCommandHandler(
        ClientImportStorageService clientImportStorageService,
        IClientImportQueue clientImportQueue,
        IClientControlContext context)
    {
        _clientImportStorageService = clientImportStorageService;
        _clientImportQueue = clientImportQueue;
        _context = context;
    }

    public async Task<ImportClientsCommandResponse> Handle(ImportClientsCommandRequest request, CancellationToken cancellationToken)
    {
        var fileStream = request.FileStream ?? throw new InvalidOperationException("O arquivo de importação não foi informado.");
        var filePath = await _clientImportStorageService.SavePendingFileAsync(fileStream, request.FileName, cancellationToken);
        var storedFileName = System.IO.Path.GetFileName(filePath);
        var clientImport = new ClientImport(request.FileName, storedFileName, request.UploadedByUserId, request.UploadedByUserName);
        await _context.ClientImports.AddAsync(clientImport, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await _clientImportQueue.QueueAsync(filePath, cancellationToken);
        return new ImportClientsCommandResponse
        {
            Message = "Arquivo enviado com sucesso. A importação será processada em background."
        };
    }
}
