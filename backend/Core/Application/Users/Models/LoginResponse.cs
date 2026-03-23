namespace Application.Users.Models;

public class LoginResponse
{
    public UserLoginResponse User { get; set; }
    public string Token { get; set; }
    public int ExpiresIn { get; set; }
}