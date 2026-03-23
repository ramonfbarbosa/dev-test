using Application.Common.Models;

namespace Persistence.Email;

public class SendGridMailRequestFactory
{
    public object Create(EmailMessage message, SendGridEmailOptions options)
    {
        return new
        {
            personalizations = new[]
            {
                new
                {
                    to = new[]
                    {
                        new
                        {
                            email = message.To.Value,
                            name = message.To.Name
                        }
                    }
                }
            },
            from = new
            {
                email = options.FromEmail,
                name = options.FromName
            },
            subject = message.Subject,
            content = new object[]
            {
                new
                {
                    type = "text/plain",
                    value = message.PlainTextContent
                },
                new
                {
                    type = "text/html",
                    value = message.HtmlContent
                }
            }
        };
    }
}
