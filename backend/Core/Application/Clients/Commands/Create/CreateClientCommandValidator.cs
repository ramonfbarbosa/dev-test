using Application.Common.Interfaces;
using Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Clients.Commands.Create;

public class CreateClientCommandValidator : ClientCommandValidatorBase<CreateClientCommandRequest>
{
    private readonly IClientControlContext _context;

    public CreateClientCommandValidator(IClientControlContext context)
    {
        _context = context;

        RuleFor(x => x.DocumentNumber)
            .MustAsync(BeUniqueDocumentNumberAsync)
            .WithMessage("Já existe um cliente com o documento informado.")
            .When(x => !string.IsNullOrWhiteSpace(x.DocumentNumber));
    }

    private async Task<bool> BeUniqueDocumentNumberAsync(
        CreateClientCommandRequest request,
        string documentNumber,
        CancellationToken cancellationToken)
    {
        var normalizedDocumentNumber = documentNumber.UnMask();
        return !await _context.Clients
            .AnyAsync(x => x.DocumentNumber == normalizedDocumentNumber, cancellationToken);
    }
}
