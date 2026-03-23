namespace Application.Users.Options;

public class UserEmailConfirmationOptions
{
    public string ConfirmationUrlBase { get; set; }
    public int TokenExpirationHours { get; set; } = 24;
}
