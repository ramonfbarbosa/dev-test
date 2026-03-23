using Application.Common.Interfaces;
using Application.Users.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Users.Commands.Update;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommandRequest>
{
    private readonly IClientControlContext _context;
    private readonly UserEmailConfirmationService _userEmailConfirmationService;

    public UpdateUserCommandHandler(IClientControlContext context, UserEmailConfirmationService userEmailConfirmationService)
    {
        _context = context;
        _userEmailConfirmationService = userEmailConfirmationService;
    }

    public async Task<Unit> Handle(UpdateUserCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        var emailChanged = user.Update(request.Username, request.Email, request.Profile);
        if (emailChanged)
        {
            await _userEmailConfirmationService.ConfigurePendingConfirmationAsync(user, cancellationToken);
        }
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
