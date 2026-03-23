using MediatR;
using System;

namespace Application.Users.Commands.ConfirmEmail;

public class ConfirmUserEmailCommandRequest : IRequest<ConfirmUserEmailCommandResponse>
{
    public Guid UserId { get; set; }
    public string Token { get; set; }
}
