using Domain;
using MediatR;
using System;

namespace Application.Users.Commands.Create;

public class CreateUserCommandRequest : IRequest<Guid>
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public Profile Profile { get; set; }
}
