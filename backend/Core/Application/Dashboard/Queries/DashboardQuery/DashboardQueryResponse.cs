using Application.Dashboard.Models;
using System.Collections.Generic;

namespace Application.Dashboard.Queries.DashboardQuery;

public class DashboardQueryResponse
{
    public int TotalClients { get; init; }
    public int ClientsThisMonth { get; init; }
    public int DistinctStates { get; init; }
    public int TotalUsers { get; init; }
    public int ActiveUsers { get; init; }
    public int UsersWithConfirmedEmail { get; init; }

    public List<ClientsByStateItem> ClientsByState { get; init; } = [];
    public List<UsersByProfileItem> UsersByProfile { get; init; } = [];
    public List<NewClientsPerMonthItem> NewClientsPerMonth { get; init; } = [];

    public List<RecentClientItem> RecentClients { get; init; } = [];
    public List<RecentUserItem> RecentUsers { get; init; } = [];
}
