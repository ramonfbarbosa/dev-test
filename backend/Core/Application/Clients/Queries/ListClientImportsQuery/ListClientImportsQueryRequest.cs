using Application.Common.Models;
using MediatR;

namespace Application.Clients.Queries.ListClientImportsQuery;

public class ListClientImportsQueryRequest : IRequest<PagedList<ListClientImportsQueryResponse>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; }
    public string SortDirection { get; set; }
}
