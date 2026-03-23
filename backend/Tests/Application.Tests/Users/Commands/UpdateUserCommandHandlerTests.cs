using Application.Users.Commands.Update;
using Application.Users.Options;
using Application.Tests.Support;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Threading;
using Application.Users.Services;

namespace Application.Tests.Users.Commands;

public class UpdateUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenEmailChanges_SendsNewConfirmationAndResetsEmailConfirmation()
    {
        await using var context = ClientControlContextFactory.Create();
        var user = TestUserFactory.Create(active: true, emailConfirmed: true, profile: Profile.Operator);
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        var provider = new RecordingEmailProvider();
        var service = CreateService(provider, "https://frontend.local/confirm-email");
        var handler = new UpdateUserCommandHandler(context, service);
        var request = new UpdateUserCommandRequest
        {
            Id = user.Id,
            Username = "updated_user",
            Email = $"updated_{user.Email}",
            Profile = Profile.Administrator
        };

        await handler.Handle(request, CancellationToken.None);

        var persistedUser = await context.Users.SingleAsync(x => x.Id == user.Id, CancellationToken.None);
        var sentMessage = Assert.Single(provider.SentMessages);

        Assert.Equal("updated_user", persistedUser.Username);
        Assert.Equal(request.Email, persistedUser.Email);
        Assert.Equal(Profile.Administrator, persistedUser.Profile);
        Assert.False(persistedUser.EmailConfirmed);
        Assert.NotNull(persistedUser.EmailConfirmationTokenHash);
        Assert.NotNull(persistedUser.EmailConfirmationTokenExpiresAt);
        Assert.Equal(request.Email, sentMessage.To.Value);
    }

    [Fact]
    public async Task Handle_WhenEmailChangesOnlyByCase_DoesNotSendNewConfirmation()
    {
        await using var context = ClientControlContextFactory.Create();
        var user = TestUserFactory.Create(active: true, emailConfirmed: true, profile: Profile.Operator);
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        var provider = new RecordingEmailProvider();
        var service = CreateService(provider, string.Empty);
        var handler = new UpdateUserCommandHandler(context, service);
        var request = new UpdateUserCommandRequest
        {
            Id = user.Id,
            Username = "updated_user",
            Email = user.Email.ToUpperInvariant(),
            Profile = Profile.Administrator
        };

        await handler.Handle(request, CancellationToken.None);

        var persistedUser = await context.Users.SingleAsync(x => x.Id == user.Id, CancellationToken.None);

        Assert.Equal("updated_user", persistedUser.Username);
        Assert.Equal(request.Email, persistedUser.Email);
        Assert.Equal(Profile.Administrator, persistedUser.Profile);
        Assert.True(persistedUser.EmailConfirmed);
        Assert.Null(persistedUser.EmailConfirmationTokenHash);
        Assert.Null(persistedUser.EmailConfirmationTokenExpiresAt);
        Assert.Empty(provider.SentMessages);
    }

    private static UserEmailConfirmationService CreateService(
        RecordingEmailProvider provider,
        string confirmationUrlBase)
    {
        return new UserEmailConfirmationService(
            provider,
            Options.Create(new UserEmailConfirmationOptions
            {
                ConfirmationUrlBase = confirmationUrlBase,
                TokenExpirationHours = 24
            }));
    }
}
