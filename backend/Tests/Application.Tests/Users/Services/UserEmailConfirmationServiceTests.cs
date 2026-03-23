using Application.Users.Helpers;
using Application.Users.Options;
using Application.Tests.Support;
using Domain;
using Domain.Entities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Application.Users.Services;

namespace Application.Tests.Users.Services;

public class UserEmailConfirmationServiceTests
{
    [Fact]
    public async Task ConfigurePendingConfirmationAsync_WhenConfirmationUrlBaseIsMissing_ThrowsInvalidOperationException()
    {
        var provider = new RecordingEmailProvider();
        var service = CreateService(provider, string.Empty);
        var user = new User("service_user", "service_user@clientcontrol.local", "hash", Profile.Operator, emailConfirmed: true, active: true);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ConfigurePendingConfirmationAsync(user, CancellationToken.None));

        Assert.Equal("A URL base de confirmação de email não está configurada.", exception.Message);
        Assert.Null(user.EmailConfirmationTokenHash);
        Assert.Null(user.EmailConfirmationTokenExpiresAt);
        Assert.Empty(provider.SentMessages);
    }

    [Fact]
    public async Task ConfigurePendingConfirmationAsync_WhenConfigured_SetsPendingStateAndSendsEmailWithMatchingToken()
    {
        var provider = new RecordingEmailProvider();
        var service = CreateService(provider, "https://frontend.local/confirm-email?origin=test");
        var user = new User("service_user", "service_user@clientcontrol.local", "hash", Profile.Operator, emailConfirmed: true, active: true);
        var startedAt = DateTime.UtcNow;

        await service.ConfigurePendingConfirmationAsync(user, CancellationToken.None);

        var finishedAt = DateTime.UtcNow;
        var sentMessage = Assert.Single(provider.SentMessages);

        Assert.False(user.EmailConfirmed);
        Assert.NotNull(user.EmailConfirmationTokenHash);
        Assert.NotNull(user.EmailConfirmationTokenExpiresAt);
        Assert.InRange(user.EmailConfirmationTokenExpiresAt!.Value, startedAt.AddHours(24), finishedAt.AddHours(24).AddSeconds(1));
        Assert.Equal(user.Email, sentMessage.To.Value);
        Assert.Equal(user.Username, sentMessage.To.Name);
        Assert.Equal("Confirme seu email", sentMessage.Subject);

        var confirmationUrl = ExtractFirstUrl(sentMessage.PlainTextContent);
        var query = ParseQuery(new Uri(confirmationUrl).Query);

        Assert.Equal("test", query["origin"]);
        Assert.Equal(user.Id.ToString(), query["userId"]);
        Assert.True(query.TryGetValue("token", out var token));
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(UserEmailConfirmationTokenHelper.HashToken(token), user.EmailConfirmationTokenHash);
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

    private static string ExtractFirstUrl(string content)
    {
        var match = Regex.Match(content, @"https?://\S+");
        Assert.True(match.Success);
        return match.Value;
    }

    private static Dictionary<string, string> ParseQuery(string queryString)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var parts = queryString.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var segments = part.Split('=', 2, StringSplitOptions.None);
            var key = Uri.UnescapeDataString(segments[0]);
            var value = segments.Length > 1
                ? Uri.UnescapeDataString(segments[1])
                : string.Empty;

            values[key] = value;
        }

        return values;
    }
}
