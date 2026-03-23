using Application.Users.Commands.Login;
using Application.Tests.Support;
using System.Linq;
using System.Threading;

namespace Application.Tests.Users.Commands;

public class LoginCommandValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WhenUserIsInactive_ReturnsInactiveError()
    {
        await using var context = ClientControlContextFactory.Create();
        var user = TestUserFactory.Create(active: false, emailConfirmed: true);
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        var validator = new LoginCommandValidator(context);
        var command = new LoginCommandRequest
        {
            Username = user.Username,
            Password = TestUserFactory.DefaultPassword
        };

        var result = await validator.ValidateAsync(command, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(LoginCommandRequest.Username) &&
            error.ErrorMessage == "Usuário desativado. Entre em contato com um administrador.");
        Assert.Null(command.ValidatedUser);
    }

    [Fact]
    public async Task ValidateAsync_WhenUserIsActiveAndConfirmed_SetsValidatedUser()
    {
        await using var context = ClientControlContextFactory.Create();
        var user = TestUserFactory.Create(active: true, emailConfirmed: true);
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        var validator = new LoginCommandValidator(context);
        var command = new LoginCommandRequest
        {
            Username = user.Username,
            Password = TestUserFactory.DefaultPassword
        };

        var result = await validator.ValidateAsync(command, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.NotNull(command.ValidatedUser);
        Assert.Equal(user.Id, command.ValidatedUser.Id);
    }

    [Fact]
    public async Task ValidateAsync_WhenEmailIsNotConfirmed_ReturnsConfirmationRequiredError()
    {
        await using var context = ClientControlContextFactory.Create();
        var user = TestUserFactory.Create(active: true, emailConfirmed: false);
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        var validator = new LoginCommandValidator(context);
        var command = new LoginCommandRequest
        {
            Username = user.Username,
            Password = TestUserFactory.DefaultPassword
        };

        var result = await validator.ValidateAsync(command, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(LoginCommandRequest.Username) &&
            error.ErrorMessage == "Confirme seu email antes de acessar o sistema.");
        Assert.Null(command.ValidatedUser);
    }

    [Fact]
    public async Task ValidateAsync_WhenPasswordIsInvalid_ReturnsInvalidCredentialsError()
    {
        await using var context = ClientControlContextFactory.Create();
        var user = TestUserFactory.Create(active: true, emailConfirmed: true);
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        var validator = new LoginCommandValidator(context);
        var command = new LoginCommandRequest
        {
            Username = user.Username,
            Password = "WrongPassword123!"
        };

        var result = await validator.ValidateAsync(command, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(LoginCommandRequest.Password) &&
            error.ErrorMessage == "Usuário ou senha inválidos.");
        Assert.Null(command.ValidatedUser);
    }

    [Fact]
    public async Task ValidateAsync_WhenUserDoesNotExist_ReturnsInvalidCredentialsError()
    {
        await using var context = ClientControlContextFactory.Create();
        var validator = new LoginCommandValidator(context);
        var command = new LoginCommandRequest
        {
            Username = "missing_user",
            Password = TestUserFactory.DefaultPassword
        };

        var result = await validator.ValidateAsync(command, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(LoginCommandRequest.Password) &&
            error.ErrorMessage == "Usuário ou senha inválidos.");
        Assert.Null(command.ValidatedUser);
    }
}
