using Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Users.Commands.Create;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommandRequest>
{
    private readonly IClientControlContext _context;

    public CreateUserCommandValidator(IClientControlContext context)
    {
        _context = context;

        RuleFor(x => x.Username)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Usuário é obrigatório.")
            .MaximumLength(50)
            .WithMessage("Usuário deve ter no máximo 50 caracteres.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email é obrigatório.")
            .MaximumLength(255)
            .WithMessage("Email deve ter no máximo 255 caracteres.")
            .EmailAddress()
            .WithMessage("Email informado é inválido.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Senha é obrigatória.")
            .MinimumLength(6)
            .WithMessage("Senha deve ter pelo menos 6 caracteres.");

        RuleFor(x => x.Profile)
            .Cascade(CascadeMode.Stop)
            .IsInEnum()
            .WithMessage("Perfil informado é inválido.");

        RuleFor(x => x.Username)
            .MustAsync(BeUniqueUsernameAsync)
            .WithMessage("Já existe um usuário com o nome informado.")
            .When(x => !string.IsNullOrWhiteSpace(x.Username));

        RuleFor(x => x.Email)
            .MustAsync(BeUniqueEmailAsync)
            .WithMessage("Já existe um usuário com o email informado.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }

    private async Task<bool> BeUniqueUsernameAsync(
        CreateUserCommandRequest request,
        string username,
        CancellationToken cancellationToken)
    {
        return !await _context.Users
            .AnyAsync(x => x.Username == username, cancellationToken);
    }

    private async Task<bool> BeUniqueEmailAsync(
        CreateUserCommandRequest request,
        string email,
        CancellationToken cancellationToken)
    {
        return !await _context.Users
            .AnyAsync(x => x.Email == email, cancellationToken);
    }
}
