using Application.Common.Models;
using MediatR;

namespace Application.Clients.Queries.FilteredClientsQuery;

public class FilteredClientsQueryRequest : IRequest<PagedList<FilteredClientsQueryResponse>>
{
    public string DocumentNumber { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; }
    public string SortDirection { get; set; }
}
