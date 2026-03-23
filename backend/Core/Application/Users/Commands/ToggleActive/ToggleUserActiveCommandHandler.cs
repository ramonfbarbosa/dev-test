using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Users.Commands.ToggleActive;

public class ToggleUserActiveCommandHandler : IRequestHandler<ToggleUserActiveCommandRequest>
{
    private readonly IClientControlContext _context;

    public ToggleUserActiveCommandHandler(IClientControlContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ToggleUserActiveCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        user.ToggleActive();
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
