using Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Users.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommandRequest>
{
    private readonly IClientControlContext _context;

    public LoginCommandValidator(IClientControlContext context)
    {
        _context = context;

        RuleFor(x => x.Username)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Usuário é obrigatório.")
            .MaximumLength(50)
            .WithMessage("Usuário deve ter no máximo 50 caracteres.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Senha é obrigatória.");

        RuleFor(x => x)
            .CustomAsync(ValidateLoginAsync);
    }

    private async Task ValidateLoginAsync(
        LoginCommandRequest request,
        ValidationContext<LoginCommandRequest> context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return;
        }
        var user = await _context.Users.SingleOrDefaultAsync(x => x.Username == request.Username, cancellationToken);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            context.AddFailure(nameof(request.Password), "Usuário ou senha inválidos.");
            return;
        }
        if (!user.Active)
        {
            context.AddFailure(nameof(request.Username), "Usuário desativado. Entre em contato com um administrador.");
            return;
        }
        if (!user.EmailConfirmed)
        {
            context.AddFailure(nameof(request.Username), "Confirme seu email antes de acessar o sistema.");
            return;
        }
        request.SetValidatedUser(user);
    }
}
