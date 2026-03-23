using Application.Clients.Imports;
using FluentValidation;
using System;
using System.IO;

namespace Application.Clients.Commands.Import;

public class ImportClientsCommandValidator : AbstractValidator<ImportClientsCommandRequest>
{
    private readonly ClientImportStorageService _clientImportStorageService;

    public ImportClientsCommandValidator(ClientImportStorageService clientImportStorageService)
    {
        _clientImportStorageService = clientImportStorageService;

        RuleFor(x => x)
            .Custom(ValidateFile);
    }

    private void ValidateFile(ImportClientsCommandRequest request, ValidationContext<ImportClientsCommandRequest> context)
    {
        if (request.FileStream is null || request.FileSizeInBytes <= 0 || string.IsNullOrWhiteSpace(request.FileName))
        {
            context.AddFailure(nameof(request.FileName), "Selecione um arquivo CSV para importação.");
            return;
        }
        if (!string.Equals(Path.GetExtension(request.FileName), ".csv", StringComparison.OrdinalIgnoreCase))
        {
            context.AddFailure(nameof(request.FileName), "A importação em lote aceita apenas arquivos CSV.");
            return;
        }
        if (request.FileSizeInBytes > _clientImportStorageService.MaxFileSizeInBytes)
        {
            context.AddFailure(
                nameof(request.FileSizeInBytes),
                $"O arquivo excede o limite de {_clientImportStorageService.MaxFileSizeInBytes / (1024 * 1024)} MB.");
        }
    }
}
