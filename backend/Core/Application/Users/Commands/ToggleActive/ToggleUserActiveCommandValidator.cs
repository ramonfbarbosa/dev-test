using Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Users.Commands.ToggleActive;

public class ToggleUserActiveCommandValidator : AbstractValidator<ToggleUserActiveCommandRequest>
{
    private readonly IClientControlContext _context;

    public ToggleUserActiveCommandValidator(IClientControlContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Usuário é obrigatório.")
            .MustAsync(ExistUserAsync)
            .WithMessage("Usuário não encontrado.");
    }

    private async Task<bool> ExistUserAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Users
            .AnyAsync(x => x.Id == id, cancellationToken);
    }
}
