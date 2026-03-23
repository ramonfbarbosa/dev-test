using Application.Common.Interfaces;
using Application.Clients.Commands.Create;
using Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Clients.Commands.Update;

public class UpdateClientCommandValidator : ClientCommandValidatorBase<UpdateClientCommandRequest>
{
    private readonly IClientControlContext _context;

    public UpdateClientCommandValidator(IClientControlContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Cliente é obrigatório");

        RuleFor(x => x.DocumentNumber)
            .MustAsync(BeUniqueDocumentNumberAsync)
            .WithMessage("Já existe um cliente com o documento informado.")
            .When(x => !string.IsNullOrWhiteSpace(x.DocumentNumber));
    }

    private async Task<bool> BeUniqueDocumentNumberAsync(
        UpdateClientCommandRequest request,
        string documentNumber,
        CancellationToken cancellationToken)
    {
        var normalizedDocumentNumber = documentNumber.UnMask();
        return !await _context.Clients
            .AnyAsync(x =>
                x.Id != request.Id &&
                x.DocumentNumber == normalizedDocumentNumber,
                cancellationToken);
    }
}
