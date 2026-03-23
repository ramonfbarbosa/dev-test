using Application.Common.Models;
using System;
using System.IO;
using System.Net;

namespace Application.Users.Helpers;

public static class UserEmailConfirmationMessageHelper
{
    private const string HtmlTemplateResourceName = "Application.Users.Template.UserEmailConfirmationTemplate.html";
    private const string BrandName = "ClientControl";
    private const string PrimaryColor = "#4F46E5";
    private const string PrimaryDarkColor = "#4338CA";
    private const string BackgroundColor = "#F4F7F9";
    private const string CardColor = "#FFFFFF";
    private const string TextColor = "#1F2937";
    private const string MutedTextColor = "#6B7280";
    private const string BorderColor = "#E2E8EE";
    private static readonly Lazy<string> HtmlTemplate = new(LoadHtmlTemplate);

    public static EmailMessage Create(string username, string email, string confirmationUrl)
    {
        return new EmailMessage
        {
            To = new EmailAddress
            {
                Name = username,
                Value = email
            },
            Subject = "Confirme seu email",
            HtmlContent = BuildHtmlContent(username, confirmationUrl),
            PlainTextContent = BuildPlainTextContent(username, confirmationUrl)
        };
    }

    private static string BuildPlainTextContent(string username, string confirmationUrl)
    {
        return
            $@"Olá {username},

            Confirme seu email para concluir o acesso ao {BrandName}.

            Acesse o link abaixo:
            {confirmationUrl}

            Se você não solicitou este cadastro, ignore esta mensagem.";
    }

    private static string BuildHtmlContent(string username, string confirmationUrl)
    {
        return HtmlTemplate.Value
            .Replace("{{BrandName}}", WebUtility.HtmlEncode(BrandName), StringComparison.Ordinal)
            .Replace("{{PrimaryColor}}", PrimaryColor, StringComparison.Ordinal)
            .Replace("{{PrimaryDarkColor}}", PrimaryDarkColor, StringComparison.Ordinal)
            .Replace("{{BackgroundColor}}", BackgroundColor, StringComparison.Ordinal)
            .Replace("{{CardColor}}", CardColor, StringComparison.Ordinal)
            .Replace("{{TextColor}}", TextColor, StringComparison.Ordinal)
            .Replace("{{MutedTextColor}}", MutedTextColor, StringComparison.Ordinal)
            .Replace("{{BorderColor}}", BorderColor, StringComparison.Ordinal)
            .Replace("{{Username}}", WebUtility.HtmlEncode(username), StringComparison.Ordinal)
            .Replace("{{ConfirmationUrl}}", WebUtility.HtmlEncode(confirmationUrl), StringComparison.Ordinal);
    }

    private static string LoadHtmlTemplate()
    {
        var assembly = typeof(UserEmailConfirmationMessageHelper).Assembly;
        using var stream = assembly.GetManifestResourceStream(HtmlTemplateResourceName)
            ?? throw new InvalidOperationException($"O template de email '{HtmlTemplateResourceName}' não foi encontrado.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
