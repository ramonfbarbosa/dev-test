using MediatR;
using System;

namespace Application.Users.Commands.ResendConfirmationEmail;

public class ResendConfirmationEmailCommandRequest : IRequest
{
    public Guid Id { get; set; }
}
