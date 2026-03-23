using MediatR;
using System;

namespace Application.Clients.Commands.Delete;

public class DeleteClientCommandRequest : IRequest
{
    public Guid Id { get; set; }
}
