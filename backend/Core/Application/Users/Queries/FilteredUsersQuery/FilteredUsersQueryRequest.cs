using Application.Common.Models;
using MediatR;

namespace Application.Users.Queries.FilteredUsersQuery;

public class FilteredUsersQueryRequest : IRequest<PagedList<FilteredUsersQueryResponse>>
{
    public string Search { get; set; }
    public int? Profile { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; }
    public string SortDirection { get; set; }
}
