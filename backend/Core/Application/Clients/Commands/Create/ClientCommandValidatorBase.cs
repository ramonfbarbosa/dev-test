using Application.Clients.Models;
using FluentValidation;

namespace Application.Clients.Commands.Create;

public abstract class ClientCommandValidatorBase<TRequest> : AbstractValidator<TRequest>
    where TRequest : ClientModel
{
    protected ClientCommandValidatorBase()
    {
        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MaximumLength(100)
            .WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Sobrenome é obrigatório")
            .MaximumLength(100)
            .WithMessage("Sobrenome deve ter no máximo 100 caracteres");

        RuleFor(x => x.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Telefone é obrigatório")
            .MaximumLength(15)
            .WithMessage("Telefone deve ter no máximo 15 caracteres");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email é obrigatório")
            .MaximumLength(255)
            .WithMessage("Email deve ter no máximo 255 caracteres")
            .EmailAddress()
            .WithMessage("Email inválido");

        RuleFor(x => x.DocumentNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Documento é obrigatório")
            .MaximumLength(20)
            .WithMessage("Documento deve ter no máximo 20 caracteres");

        RuleFor(x => x.BirthDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Data de nascimento é obrigatória");

        RuleFor(x => x.Address)
            .NotNull()
            .WithMessage("Endereço é obrigatório");

        When(x => x.Address != null, () =>
        {
            RuleFor(x => x.Address.PostalCode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("CEP é obrigatório")
                .MaximumLength(10)
                .WithMessage("CEP deve ter no máximo 10 caracteres");

            RuleFor(x => x.Address.AddressLine)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Endereço é obrigatório")
                .MaximumLength(200)
                .WithMessage("Endereço deve ter no máximo 200 caracteres");

            RuleFor(x => x.Address.Number)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Número é obrigatório")
                .MaximumLength(10)
                .WithMessage("Número deve ter no máximo 10 caracteres");

            RuleFor(x => x.Address.Complement)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(100)
                .WithMessage("Complemento deve ter no máximo 100 caracteres");

            RuleFor(x => x.Address.Neighborhood)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Bairro é obrigatório")
                .MaximumLength(100)
                .WithMessage("Bairro deve ter no máximo 100 caracteres");

            RuleFor(x => x.Address.City)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Cidade é obrigatória")
                .MaximumLength(100)
                .WithMessage("Cidade deve ter no máximo 100 caracteres");

            RuleFor(x => x.Address.State)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Estado é obrigatório")
                .MaximumLength(2)
                .WithMessage("Estado deve ter no máximo 2 caracteres");
        });
    }
}
