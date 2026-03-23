namespace Application.Common.Models;

public class EmailMessage
{
    public EmailAddress To { get; set; }
    public string Subject { get; set; }
    public string HtmlContent { get; set; }
    public string PlainTextContent { get; set; }
}
