using Application.Users.Helpers;
using System;

namespace Application.Tests.Users.Helpers;

public class UserEmailConfirmationMessageHelperTests
{
    [Fact]
    public void Create_WhenBuildingHtml_RemovesLogoAndKeepsHeaderTextReadable()
    {
        var message = UserEmailConfirmationMessageHelper.Create(
            username: "Ramon",
            email: "ramon@clientcontrol.local",
            confirmationUrl: "https://frontend.local/confirm-email?token=abc");

        Assert.DoesNotContain("<svg", message.HtmlContent, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("background:linear-gradient", message.HtmlContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ClientControl", message.HtmlContent, StringComparison.Ordinal);
        Assert.Contains("<font color=\"#000000\">Confirme seu email</font>", message.HtmlContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<font color=\"#000000\">Falta só um passo para liberar seu acesso ao sistema.</font>", message.HtmlContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("bgcolor=\"#FFFFFF\"", message.HtmlContent, StringComparison.OrdinalIgnoreCase);
    }
}
