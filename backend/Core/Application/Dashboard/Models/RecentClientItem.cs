using System;

namespace Application.Dashboard.Models;

public class RecentClientItem
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DocumentNumber { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string CityState { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
