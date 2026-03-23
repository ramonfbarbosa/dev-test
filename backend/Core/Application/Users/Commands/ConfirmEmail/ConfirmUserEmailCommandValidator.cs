using Application.Common.Interfaces;
using Application.Users.Helpers;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Users.Commands.ConfirmEmail;

public class ConfirmUserEmailCommandValidator : AbstractValidator<ConfirmUserEmailCommandRequest>
{
    private readonly IClientControlContext _context;

    public ConfirmUserEmailCommandValidator(IClientControlContext context)
    {
        _context = context;

        RuleFor(x => x.UserId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Usuário é obrigatório.");

        RuleFor(x => x.Token)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Token de confirmação é obrigatório.");

        RuleFor(x => x)
            .CustomAsync(ValidateConfirmationAsync);
    }

    private async Task ValidateConfirmationAsync(
        ConfirmUserEmailCommandRequest request,
        ValidationContext<ConfirmUserEmailCommandRequest> context,
        CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty || string.IsNullOrWhiteSpace(request.Token))
        {
            return;
        }
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            context.AddFailure(nameof(request.UserId), "Usuário não encontrado.");
            return;
        }
        if (user.EmailConfirmed)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(user.EmailConfirmationTokenHash) || user.EmailConfirmationTokenExpiresAt is null)
        {
            context.AddFailure(nameof(request.Token), "O link de confirmação é inválido.");
            return;
        }
        if (user.EmailConfirmationTokenExpiresAt.Value < DateTime.UtcNow)
        {
            context.AddFailure(nameof(request.Token), "O link de confirmação expirou.");
            return;
        }
        var tokenHash = UserEmailConfirmationTokenHelper.HashToken(request.Token);
        if (!string.Equals(user.EmailConfirmationTokenHash, tokenHash, StringComparison.Ordinal))
        {
            context.AddFailure(nameof(request.Token), "O link de confirmação é inválido.");
        }
    }
}
