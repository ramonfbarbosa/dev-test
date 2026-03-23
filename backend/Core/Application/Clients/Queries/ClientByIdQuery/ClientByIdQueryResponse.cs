using Application.Clients.Models;
using System;

namespace Application.Clients.Queries.ClientByIdQuery
{
    public class ClientByIdQueryResponse : ClientModel
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
