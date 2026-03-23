using Application.Clients.Commands.Create;
using Application.Tests.Support;
using System.Linq;
using System.Threading;

namespace Application.Tests.Clients.Commands;

public class CreateClientCommandValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WhenAllFieldsAreValid_IsValid()
    {
        await using var context = ClientControlContextFactory.Create();
        var validator = new CreateClientCommandValidator(context);
        var request = TestClientFactory.CreateRequest();

        var result = await validator.ValidateAsync(request, CancellationToken.None);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_WhenFirstNameIsEmpty_ReturnsError()
    {
        await using var context = ClientControlContextFactory.Create();
        var validator = new CreateClientCommandValidator(context);
        var request = TestClientFactory.CreateRequest();
        request.FirstName = "";

        var result = await validator.ValidateAsync(request, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(CreateClientCommandRequest.FirstName) &&
            e.ErrorMessage == "Nome é obrigatório");
    }

    [Fact]
    public async Task ValidateAsync_WhenEmailIsInvalid_ReturnsError()
    {
        await using var context = ClientControlContextFactory.Create();
        var validator = new CreateClientCommandValidator(context);
        var request = TestClientFactory.CreateRequest();
        request.Email = "not-an-email";

        var result = await validator.ValidateAsync(request, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(CreateClientCommandRequest.Email) &&
            e.ErrorMessage == "Email inválido");
    }

    [Fact]
    public async Task ValidateAsync_WhenDocumentNumberIsDuplicate_ReturnsError()
    {
        await using var context = ClientControlContextFactory.Create();
        var existing = TestClientFactory.CreateEntity(documentNumber: "12345678900");
        context.Clients.Add(existing);
        await context.SaveChangesAsync(CancellationToken.None);

        var validator = new CreateClientCommandValidator(context);
        var request = TestClientFactory.CreateRequest(documentNumber: "12345678900");

        var result = await validator.ValidateAsync(request, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(CreateClientCommandRequest.DocumentNumber) &&
            e.ErrorMessage == "Já existe um cliente com o documento informado.");
    }

    [Fact]
    public async Task ValidateAsync_WhenAddressIsNull_ReturnsError()
    {
        await using var context = ClientControlContextFactory.Create();
        var validator = new CreateClientCommandValidator(context);
        var request = TestClientFactory.CreateRequest();
        request.Address = null;

        var result = await validator.ValidateAsync(request, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(CreateClientCommandRequest.Address) &&
            e.ErrorMessage == "Endereço é obrigatório");
    }

    [Fact]
    public async Task ValidateAsync_WhenPostalCodeIsEmpty_ReturnsError()
    {
        await using var context = ClientControlContextFactory.Create();
        var validator = new CreateClientCommandValidator(context);
        var request = TestClientFactory.CreateRequest();
        request.Address.PostalCode = "";

        var result = await validator.ValidateAsync(request, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "Address.PostalCode" &&
            e.ErrorMessage == "CEP é obrigatório");
    }

    [Fact]
    public async Task ValidateAsync_WhenStateExceedsMaxLength_ReturnsError()
    {
        await using var context = ClientControlContextFactory.Create();
        var validator = new CreateClientCommandValidator(context);
        var request = TestClientFactory.CreateRequest();
        request.Address.State = "ABC";

        var result = await validator.ValidateAsync(request, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "Address.State" &&
            e.ErrorMessage == "Estado deve ter no máximo 2 caracteres");
    }
}
