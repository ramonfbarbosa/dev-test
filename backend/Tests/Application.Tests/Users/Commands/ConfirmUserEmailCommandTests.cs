using Application.Users.Commands.ConfirmEmail;
using Application.Users.Helpers;
using Application.Tests.Support;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;

namespace Application.Tests.Users.Commands;

public class ConfirmUserEmailCommandTests
{
    [Fact]
    public async Task ValidateAsync_WhenTokenExpired_ReturnsExpirationError()
    {
        await using var context = ClientControlContextFactory.Create();
        const string token = "expired-token";
        var user = TestUserFactory.Create(active: false, emailConfirmed: false);
        user.SetEmailConfirmation(
            UserEmailConfirmationTokenHelper.HashToken(token),
            DateTime.UtcNow.AddMinutes(-5));
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        var validator = new ConfirmUserEmailCommandValidator(context);

        var result = await validator.ValidateAsync(new ConfirmUserEmailCommandRequest
        {
            UserId = user.Id,
            Token = token
        }, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(ConfirmUserEmailCommandRequest.Token) &&
            error.ErrorMessage == "O link de confirmação expirou.");
    }

    [Fact]
    public async Task ValidateAsync_WhenTokenIsInvalid_ReturnsInvalidLinkError()
    {
        await using var context = ClientControlContextFactory.Create();
        const string validToken = "expected-token";
        var user = TestUserFactory.Create(active: false, emailConfirmed: false);
        user.SetEmailConfirmation(
            UserEmailConfirmationTokenHelper.HashToken(validToken),
            DateTime.UtcNow.AddMinutes(30));
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        var validator = new ConfirmUserEmailCommandValidator(context);

        var result = await validator.ValidateAsync(new ConfirmUserEmailCommandRequest
        {
            UserId = user.Id,
            Token = "tampered-token"
        }, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(ConfirmUserEmailCommandRequest.Token) &&
            error.ErrorMessage == "O link de confirmação é inválido.");
    }

    [Fact]
    public async Task ValidateAsync_WhenEmailIsAlreadyConfirmed_AllowsRequestWithoutTokenState()
    {
        await using var context = ClientControlContextFactory.Create();
        var user = TestUserFactory.Create(active: true, emailConfirmed: true);
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        var validator = new ConfirmUserEmailCommandValidator(context);

        var result = await validator.ValidateAsync(new ConfirmUserEmailCommandRequest
        {
            UserId = user.Id,
            Token = "ignored-token"
        }, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Handle_WhenConfirmationIsValid_ConfirmsEmailActivatesUserAndClearsPendingToken()
    {
        await using var context = ClientControlContextFactory.Create();
        const string token = "valid-token";
        var user = TestUserFactory.Create(active: false, emailConfirmed: false);
        user.SetEmailConfirmation(
            UserEmailConfirmationTokenHelper.HashToken(token),
            DateTime.UtcNow.AddMinutes(30));
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new ConfirmUserEmailCommandHandler(context);

        var response = await handler.Handle(new ConfirmUserEmailCommandRequest
        {
            UserId = user.Id,
            Token = token
        }, CancellationToken.None);

        var persistedUser = await context.Users.SingleAsync(x => x.Id == user.Id, CancellationToken.None);

        Assert.Equal("Email confirmado com sucesso.", response.Message);
        Assert.True(persistedUser.EmailConfirmed);
        Assert.True(persistedUser.Active);
        Assert.Null(persistedUser.EmailConfirmationTokenHash);
        Assert.Null(persistedUser.EmailConfirmationTokenExpiresAt);
    }
}
