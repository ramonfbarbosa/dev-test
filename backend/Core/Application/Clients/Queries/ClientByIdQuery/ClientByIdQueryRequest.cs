using MediatR;
using System;

namespace Application.Clients.Queries.ClientByIdQuery;

public class ClientByIdQueryRequest : IRequest<ClientByIdQueryResponse>
{
    public Guid Id { get; set; }
}
