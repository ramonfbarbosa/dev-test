namespace Persistence.Email;

public class SendGridEmailOptions
{
    public string ApiKey { get; set; }
    public string FromEmail { get; set; }
    public string FromName { get; set; }
    public string BaseUrl { get; set; }
}
