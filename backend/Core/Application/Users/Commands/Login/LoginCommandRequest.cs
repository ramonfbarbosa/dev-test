using MediatR;
using Application.Users.Models;
using Domain.Entities;

namespace Application.Users.Commands.Login;

public class LoginCommandRequest : IRequest<LoginResponse>
{
    public string Username { get; set; }
    public string Password { get; set; }

    public User ValidatedUser { get; private set; }

    public void SetValidatedUser(User user)
    {
        ValidatedUser = user;
    }
}
