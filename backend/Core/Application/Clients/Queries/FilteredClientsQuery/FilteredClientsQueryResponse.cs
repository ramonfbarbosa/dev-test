using Application.Clients.Models;
using System;

namespace Application.Clients.Queries.FilteredClientsQuery;

public class FilteredClientsQueryResponse : ClientModel
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
}
