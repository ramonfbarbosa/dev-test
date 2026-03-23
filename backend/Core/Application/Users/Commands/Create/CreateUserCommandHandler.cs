using Application.Common.Interfaces;
using Application.Users.Services;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Users.Commands.Create;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommandRequest, Guid>
{
    private readonly IClientControlContext _context;
    private readonly UserEmailConfirmationService _userEmailConfirmationService;

    public CreateUserCommandHandler(IClientControlContext context, UserEmailConfirmationService userEmailConfirmationService)
    {
        _context = context;
        _userEmailConfirmationService = userEmailConfirmationService;
    }

    public async Task<Guid> Handle(CreateUserCommandRequest request, CancellationToken cancellationToken)
    {
        var user = new User
        (
            request.Username,
            request.Email,
            BCrypt.Net.BCrypt.HashPassword(request.Password),
            request.Profile
        );
        await _userEmailConfirmationService.ConfigurePendingConfirmationAsync(user, cancellationToken);
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}
