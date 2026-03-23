using Application.Common.Interfaces;
using Application.Dashboard.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Dashboard.Queries.DashboardQuery;

public class DashboardQueryHandler : IRequestHandler<DashboardQueryRequest, DashboardQueryResponse>
{
    private readonly IClientControlContext _context;

    public DashboardQueryHandler(IClientControlContext context)
    {
        _context = context;
    }

    public async Task<DashboardQueryResponse> Handle(DashboardQueryRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var twelveMonthsAgo = startOfMonth.AddMonths(-11);

        var totalClients = await _context.Clients.CountAsync(cancellationToken);
        var clientsThisMonth = await _context.Clients.CountAsync(c => c.CreatedAt >= startOfMonth, cancellationToken);
        var distinctStates = await CountDistinctStatesAsync(cancellationToken);

        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var activeUsers = await _context.Users.CountAsync(u => u.Active, cancellationToken);
        var usersWithConfirmedEmail = await _context.Users.CountAsync(u => u.EmailConfirmed, cancellationToken);

        var clientsByState = await GetClientsByStateAsync(cancellationToken);
        var usersByProfile = await GetUsersByProfileAsync(cancellationToken);
        var newClientsPerMonth = await GetNewClientsPerMonthAsync(twelveMonthsAgo, cancellationToken);

        var recentClients = await GetRecentClientsAsync(cancellationToken);
        var recentUsers = await GetRecentUsersAsync(cancellationToken);

        return new DashboardQueryResponse
        {
            TotalClients = totalClients,
            ClientsThisMonth = clientsThisMonth,
            DistinctStates = distinctStates,
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            UsersWithConfirmedEmail = usersWithConfirmedEmail,
            ClientsByState = clientsByState,
            UsersByProfile = usersByProfile,
            NewClientsPerMonth = newClientsPerMonth,
            RecentClients = recentClients,
            RecentUsers = recentUsers,
        };
    }

    private async Task<int> CountDistinctStatesAsync(CancellationToken cancellationToken)
    {
        return await _context.Clients
            .Where(c => c.Address != null && c.Address.State != null && c.Address.State != "")
            .Select(c => c.Address.State)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    private async Task<List<ClientsByStateItem>> GetClientsByStateAsync(CancellationToken cancellationToken)
    {
        return await _context.Clients
            .Where(c => c.Address != null && c.Address.State != null && c.Address.State != "")
            .GroupBy(c => c.Address.State)
            .Select(g => new ClientsByStateItem { State = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<UsersByProfileItem>> GetUsersByProfileAsync(CancellationToken cancellationToken)
    {
        return await _context.Users
            .GroupBy(u => u.Profile)
            .Select(g => new UsersByProfileItem { Profile = g.Key.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<NewClientsPerMonthItem>> GetNewClientsPerMonthAsync(DateTime since, CancellationToken cancellationToken)
    {
        var monthlyData = await _context.Clients
            .Where(c => c.CreatedAt >= since)
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        return monthlyData
            .Select(x => new NewClientsPerMonthItem
            {
                Month = $"{x.Year}-{x.Month:D2}",
                Count = x.Count
            })
            .ToList();
    }

    private async Task<List<RecentClientItem>> GetRecentClientsAsync(CancellationToken cancellationToken)
    {
        return await _context.Clients
            .OrderByDescending(c => c.CreatedAt)
            .Take(10)
            .Select(c => new RecentClientItem
            {
                Id = c.Id,
                FullName = c.FirstName + " " + c.LastName,
                Email = c.Email,
                DocumentNumber = c.DocumentNumber,
                PhoneNumber = c.PhoneNumber,
                CityState = c.Address != null
                    ? (c.Address.City ?? "") + (c.Address.State != null ? "/" + c.Address.State : "")
                    : "",
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<RecentUserItem>> GetRecentUsersAsync(CancellationToken cancellationToken)
    {
        return await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(10)
            .Select(u => new RecentUserItem
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Profile = u.Profile.ToString(),
                EmailConfirmed = u.EmailConfirmed,
                Active = u.Active,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
