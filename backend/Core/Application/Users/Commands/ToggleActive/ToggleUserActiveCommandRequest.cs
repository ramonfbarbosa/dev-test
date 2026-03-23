using MediatR;
using System;

namespace Application.Users.Commands.ToggleActive;

public class ToggleUserActiveCommandRequest : IRequest
{
    public Guid Id { get; set; }
}
