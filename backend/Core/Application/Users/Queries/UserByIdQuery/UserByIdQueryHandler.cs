using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Users.Queries.UserByIdQuery;

public class UserByIdQueryHandler : IRequestHandler<UserByIdQueryRequest, UserByIdQueryResponse>
{
    private readonly IClientControlContext _context;

    public UserByIdQueryHandler(IClientControlContext context)
    {
        _context = context;
    }

    public async Task<UserByIdQueryResponse> Handle(UserByIdQueryRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Where(u => u.Id == request.Id)
            .Select(u => new UserByIdQueryResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                EmailConfirmed = u.EmailConfirmed,
                Active = u.Active,
                Profile = u.Profile.ToString()
            }).FirstOrDefaultAsync(cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), request.Id);
        }
        return user;
    }
}
