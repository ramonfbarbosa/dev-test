using Application.Common.Interfaces;
using Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Clients.Commands.Update;

public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommandRequest, Guid>
{
    private readonly IClientControlContext _context;

    public UpdateClientCommandHandler(IClientControlContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(UpdateClientCommandRequest request, CancellationToken cancellationToken)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(x => x.Id == request.Id);

        client.Update(
            request.FirstName,
            request.LastName,
            request.PhoneNumber.UnMask(),
            request.Email,
            request.DocumentNumber.UnMask(),
            request.BirthDate,
            new Address(
                request.Address.PostalCode.UnMask(),
                request.Address.AddressLine,
                request.Address.Number,
                request.Address.Complement,
                request.Address.Neighborhood,
                request.Address.City,
                request.Address.State));

        await _context.SaveChangesAsync(cancellationToken);

        return client.Id;
    }
}
