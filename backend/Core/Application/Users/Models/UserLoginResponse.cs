using Domain;

namespace Application.Users.Models;

public class UserLoginResponse
{
    public string Username { get; set; }
    public Profile Profile { get; set; }
}
