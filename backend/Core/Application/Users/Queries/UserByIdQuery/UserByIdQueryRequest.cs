using MediatR;
using System;

namespace Application.Users.Queries.UserByIdQuery;

public class UserByIdQueryRequest : IRequest<UserByIdQueryResponse>
{
    public Guid Id { get; set; }
}
