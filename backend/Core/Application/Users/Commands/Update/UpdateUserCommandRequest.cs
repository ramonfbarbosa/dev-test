
using Domain;
using MediatR;
using System;

namespace Application.Users.Commands.Update;

public class UpdateUserCommandRequest : IRequest
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public Profile Profile { get; set; }
}
