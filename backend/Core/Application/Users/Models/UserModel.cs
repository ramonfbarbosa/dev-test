using System;

namespace Application.Users.Models;

public class UserModel
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Role { get; set; }
}
