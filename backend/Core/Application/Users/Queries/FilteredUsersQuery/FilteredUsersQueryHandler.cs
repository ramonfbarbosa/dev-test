using Application.Common.Interfaces;
using Application.Common.Models;
using Domain;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Users.Queries.FilteredUsersQuery;

public class FilteredUsersQueryHandler : IRequestHandler<FilteredUsersQueryRequest, PagedList<FilteredUsersQueryResponse>>
{
    private readonly IClientControlContext _context;

    public FilteredUsersQueryHandler(IClientControlContext context)
    {
        _context = context;
    }

    public async Task<PagedList<FilteredUsersQueryResponse>> Handle(FilteredUsersQueryRequest request, CancellationToken cancellationToken)
    {
        var filteredUsers = ApplyFilters(_context.Users.AsQueryable(), request);
        var totalCount = await filteredUsers.CountAsync(cancellationToken);

        var sortedUsers = ApplySorting(filteredUsers, request.SortBy, request.SortDirection);

        var pagedUsers = await sortedUsers
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new FilteredUsersQueryResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                EmailConfirmed = u.EmailConfirmed,
                Active = u.Active,
                Profile = u.Profile.ToString()
            })
            .ToListAsync(cancellationToken);

        return PagedList<FilteredUsersQueryResponse>.Create(pagedUsers, totalCount, request.Page, request.PageSize);
    }

    private static IQueryable<User> ApplyFilters(IQueryable<User> query, FilteredUsersQueryRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(searchTerm) ||
                u.Email.ToLower().Contains(searchTerm));
        }

        if (request.Profile.HasValue)
        {
            var profileFilter = (Profile)request.Profile.Value;
            query = query.Where(u => u.Profile == profileFilter);
        }

        if (request.IsActive.HasValue)
            query = query.Where(u => u.Active == request.IsActive.Value);

        return query;
    }

    private static IOrderedQueryable<User> ApplySorting(IQueryable<User> query, string sortBy, string sortDirection)
    {
        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLower() switch
        {
            "email" => isDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "profile" => isDescending ? query.OrderByDescending(u => u.Profile) : query.OrderBy(u => u.Profile),
            "emailconfirmed" => isDescending ? query.OrderByDescending(u => u.EmailConfirmed) : query.OrderBy(u => u.EmailConfirmed),
            "active" => isDescending ? query.OrderByDescending(u => u.Active) : query.OrderBy(u => u.Active),
            _ => isDescending ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
        };
    }
}
