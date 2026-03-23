using System;

namespace Application.Dashboard.Models;

public class RecentUserItem
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Profile { get; init; } = string.Empty;
    public bool EmailConfirmed { get; init; }
    public bool Active { get; init; }
    public DateTime CreatedAt { get; init; }
}
