using Application.Clients.Models;
using MediatR;
using System;

namespace Application.Clients.Commands.Create;

public class CreateClientCommandRequest : ClientModel, IRequest<Guid>
{
}
