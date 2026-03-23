using Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Clients.Commands.Delete;

public class DeleteClientCommandValidator : AbstractValidator<DeleteClientCommandRequest>
{
    private readonly IClientControlContext _context;

    public DeleteClientCommandValidator(IClientControlContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Cliente é obrigatório.")
            .MustAsync(Exists)
            .WithMessage("Cliente não encontrado.");
    }

    private async Task<bool> Exists(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Clients
            .AnyAsync(x => x.Id == id, cancellationToken);
    }
}
