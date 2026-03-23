using Application.Clients.Models;
using MediatR;
using System;

namespace Application.Clients.Commands.Update;

public class UpdateClientCommandRequest : ClientModel, IRequest<Guid>
{
    public Guid Id { get; set; }
}
