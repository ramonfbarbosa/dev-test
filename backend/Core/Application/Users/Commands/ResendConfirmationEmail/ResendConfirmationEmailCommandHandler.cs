using Application.Common.Interfaces;
using Application.Users.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Users.Commands.ResendConfirmationEmail;

public class ResendConfirmationEmailCommandHandler : IRequestHandler<ResendConfirmationEmailCommandRequest>
{
    private readonly IClientControlContext _context;
    private readonly UserEmailConfirmationService _userEmailConfirmationService;

    public ResendConfirmationEmailCommandHandler(
        IClientControlContext context,
        UserEmailConfirmationService userEmailConfirmationService)
    {
        _context = context;
        _userEmailConfirmationService = userEmailConfirmationService;
    }

    public async Task<Unit> Handle(ResendConfirmationEmailCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        await _userEmailConfirmationService.ConfigurePendingConfirmationAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
