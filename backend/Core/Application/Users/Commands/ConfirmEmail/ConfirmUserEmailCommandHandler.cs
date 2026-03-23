using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Users.Commands.ConfirmEmail;

public class ConfirmUserEmailCommandHandler : IRequestHandler<ConfirmUserEmailCommandRequest, ConfirmUserEmailCommandResponse>
{
    private readonly IClientControlContext _context;

    public ConfirmUserEmailCommandHandler(IClientControlContext context)
    {
        _context = context;
    }

    public async Task<ConfirmUserEmailCommandResponse> Handle(
        ConfirmUserEmailCommandRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        user.ConfirmEmail();
        await _context.SaveChangesAsync(cancellationToken);
        return new ConfirmUserEmailCommandResponse
        {
            Message = "Email confirmado com sucesso."
        };
    }
}
