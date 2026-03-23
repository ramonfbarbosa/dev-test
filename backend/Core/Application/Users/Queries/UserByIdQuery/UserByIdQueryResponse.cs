using System;

namespace Application.Users.Queries.UserByIdQuery;

public class UserByIdQueryResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool Active { get; set; }
    public string Profile { get; set; }
}
