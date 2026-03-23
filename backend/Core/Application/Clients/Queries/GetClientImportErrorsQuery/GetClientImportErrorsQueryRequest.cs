using MediatR;
using System;

namespace Application.Clients.Queries.GetClientImportErrorsQuery;

public class GetClientImportErrorsQueryRequest : IRequest<GetClientImportErrorsQueryResponse>
{
    public Guid Id { get; set; }
}
