using Application.Common.Interfaces;
using Application.Users.Helpers;
using Application.Users.Options;
using Domain.Entities;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Users.Services;

public class UserEmailConfirmationService
{
    private readonly IEmailProvider _emailProvider;
    private readonly UserEmailConfirmationOptions _options;

    public UserEmailConfirmationService(
        IEmailProvider emailProvider,
        IOptions<UserEmailConfirmationOptions> options)
    {
        _emailProvider = emailProvider;
        _options = options.Value;
    }

    public async Task ConfigurePendingConfirmationAsync(User user, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ConfirmationUrlBase))
        {
            throw new InvalidOperationException("A URL base de confirmação de email não está configurada.");
        }
        var token = UserEmailConfirmationTokenHelper.GenerateToken();
        var tokenHash = UserEmailConfirmationTokenHelper.HashToken(token);
        var expiration = DateTime.UtcNow.AddHours(_options.TokenExpirationHours);
        user.SetEmailConfirmation(tokenHash, expiration);
        await _emailProvider.SendAsync
        (
            message: UserEmailConfirmationMessageHelper.Create
            (
                username: user.Username,
                email: user.Email,
                confirmationUrl: BuildConfirmationUrl(user.Id, token)
            ),
            cancellationToken: cancellationToken
        );
    }

    private string BuildConfirmationUrl(Guid userId, string token)
    {
        var separator = _options.ConfirmationUrlBase.Contains('?') ? "&" : "?";
        return $"{_options.ConfirmationUrlBase}{separator}userId=" +
            $"{Uri.EscapeDataString(userId.ToString())}&token={Uri.EscapeDataString(token)}";
    }
}
