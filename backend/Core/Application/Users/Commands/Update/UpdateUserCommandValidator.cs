using Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Users.Commands.Update;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommandRequest>
{
    private readonly IClientControlContext _context;

    public UpdateUserCommandValidator(IClientControlContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Usuário é obrigatório.")
            .MustAsync(ExistUserAsync)
            .WithMessage("Usuário não encontrado.");

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
        UpdateUserCommandRequest request,
        string username,
        CancellationToken cancellationToken)
    {
        return !await _context.Users
            .AnyAsync(x =>
                x.Id != request.Id &&
                x.Username == username,
                cancellationToken);
    }

    private async Task<bool> BeUniqueEmailAsync(
        UpdateUserCommandRequest request,
        string email,
        CancellationToken cancellationToken)
    {
        return !await _context.Users
            .AnyAsync(x =>
                x.Id != request.Id &&
                x.Email == email,
                cancellationToken);
    }

    private async Task<bool> ExistUserAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Users
            .AnyAsync(x => x.Id == id, cancellationToken);
    }
}
