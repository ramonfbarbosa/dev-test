using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Persistence.Email;

public class SendGridEmailProvider : IEmailProvider
{
    private readonly HttpClient _httpClient;
    private readonly SendGridMailRequestFactory _requestFactory;
    private readonly SendGridEmailOptions _options;

    public SendGridEmailProvider(
        HttpClient httpClient,
        SendGridMailRequestFactory requestFactory,
        IOptions<SendGridEmailOptions> options)
    {
        _httpClient = httpClient;
        _requestFactory = requestFactory;
        _options = options.Value;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync
        (
            requestUri: "v3/mail/send",
            value: _requestFactory.Create(message, _options),
            cancellationToken: cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return;
        }
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException
        (
            message: BuildFailureMessage((int)response.StatusCode, responseContent)
        );
    }

    private static string BuildFailureMessage(int statusCode, string responseContent)
    {
        var baseMessage = $"Não foi possível enviar o email. O SendGrid retornou o status {statusCode}.";
        return string.IsNullOrWhiteSpace(responseContent)
            ? baseMessage
            : $"{baseMessage} Detalhes: {responseContent}";
    }
}
