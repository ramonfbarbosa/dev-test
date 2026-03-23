using Application.Common.Interfaces;
using Common;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Clients.Commands.Create;

public class CreateClientCommandHandler : IRequestHandler<CreateClientCommandRequest, Guid>
{
    private readonly IClientControlContext _context;

    public CreateClientCommandHandler(IClientControlContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateClientCommandRequest request, CancellationToken cancellationToken)
    {
        var client = new Client(
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

        await _context.Clients.AddAsync(client, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return client.Id;
    }
}
