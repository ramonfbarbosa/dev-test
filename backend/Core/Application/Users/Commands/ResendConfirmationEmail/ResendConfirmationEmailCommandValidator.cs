using Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Users.Commands.ResendConfirmationEmail;

public class ResendConfirmationEmailCommandValidator : AbstractValidator<ResendConfirmationEmailCommandRequest>
{
    private readonly IClientControlContext _context;

    public ResendConfirmationEmailCommandValidator(IClientControlContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Usuário é obrigatório.")
            .MustAsync(ExistUserAsync)
            .WithMessage("Usuário não encontrado.")
            .MustAsync(EmailNotConfirmedAsync)
            .WithMessage("O email deste usuário já foi confirmado.");
    }

    private async Task<bool> ExistUserAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Users.AnyAsync(x => x.Id == id, cancellationToken);
    }

    private async Task<bool> EmailNotConfirmedAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return user != null && !user.EmailConfirmed;
    }
}
