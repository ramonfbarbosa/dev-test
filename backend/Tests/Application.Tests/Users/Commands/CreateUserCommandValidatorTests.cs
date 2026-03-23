using Application.Tests.Support;
using Application.Users.Commands.Create;
using Domain;

namespace Application.Tests.Users.Commands;

public class CreateUserCommandValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WhenAllFieldsAreValid_IsValid()
    {
        await using var context = ClientControlContextFactory.Create();
        var validator = new CreateUserCommandValidator(context);
        var request = new CreateUserCommandRequest
        {
            Username = "new_user",
            Email = "new_user@test.local",
            Password = "Secure123!",
            Profile = Profile.Operator
        };

        var result = await validator.ValidateAsync(request, CancellationToken.None);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_WhenUsernameIsEmpty_ReturnsError()
    {
        await using var context = ClientControlContextFactory.Create();
        var validator = new CreateUserCommandValidator(context);
        var request = new CreateUserCommandRequest
        {
            Username = "",
            Email = "user@test.local",
            Password = "Secure123!",
            Profile = Profile.Operator
        };

        var result = await validator.ValidateAsync(request, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(CreateUserCommandRequest.Username) &&
            e.ErrorMessage == "Usuário é obrigatório.");
    }

    [Fact]
    public async Task ValidateAsync_WhenPasswordIsTooShort_ReturnsError()
    {
        await using var context = ClientControlContextFactory.Create();
        var validator = new CreateUserCommandValidator(context);
        var request = new CreateUserCommandRequest
        {
            Username = "user",
            Email = "user@test.local",
            Password = "abc",
            Profile = Profile.Operator
        };

        var result = await validator.ValidateAsync(request, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(CreateUserCommandRequest.Password) &&
            e.ErrorMessage == "Senha deve ter pelo menos 6 caracteres.");
    }

    [Fact]
    public async Task ValidateAsync_WhenEmailIsInvalid_ReturnsError()
    {
        await using var context = ClientControlContextFactory.Create();
        var validator = new CreateUserCommandValidator(context);
        var request = new CreateUserCommandRequest
        {
            Username = "user",
            Email = "not-an-email",
            Password = "Secure123!",
            Profile = Profile.Operator
        };

        var result = await validator.ValidateAsync(request, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(CreateUserCommandRequest.Email) &&
            e.ErrorMessage == "Email informado é inválido.");
    }

    [Fact]
    public async Task ValidateAsync_WhenUsernameIsDuplicate_ReturnsError()
    {
        await using var context = ClientControlContextFactory.Create();
        var existing = TestUserFactory.Create();
        context.Users.Add(existing);
        await context.SaveChangesAsync(CancellationToken.None);

        var validator = new CreateUserCommandValidator(context);
        var request = new CreateUserCommandRequest
        {
            Username = existing.Username,
            Email = "different@test.local",
            Password = "Secure123!",
            Profile = Profile.Operator
        };

        var result = await validator.ValidateAsync(request, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(CreateUserCommandRequest.Username) &&
            e.ErrorMessage == "Já existe um usuário com o nome informado.");
    }

    [Fact]
    public async Task ValidateAsync_WhenEmailIsDuplicate_ReturnsError()
    {
        await using var context = ClientControlContextFactory.Create();
        var existing = TestUserFactory.Create();
        context.Users.Add(existing);
        await context.SaveChangesAsync(CancellationToken.None);

        var validator = new CreateUserCommandValidator(context);
        var request = new CreateUserCommandRequest
        {
            Username = "unique_user",
            Email = existing.Email,
            Password = "Secure123!",
            Profile = Profile.Operator
        };

        var result = await validator.ValidateAsync(request, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(CreateUserCommandRequest.Email) &&
            e.ErrorMessage == "Já existe um usuário com o email informado.");
    }

    [Fact]
    public async Task ValidateAsync_WhenProfileIsInvalid_ReturnsError()
    {
        await using var context = ClientControlContextFactory.Create();
        var validator = new CreateUserCommandValidator(context);
        var request = new CreateUserCommandRequest
        {
            Username = "user",
            Email = "user@test.local",
            Password = "Secure123!",
            Profile = (Profile)999
        };

        var result = await validator.ValidateAsync(request, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(CreateUserCommandRequest.Profile) &&
            e.ErrorMessage == "Perfil informado é inválido.");
    }
}
