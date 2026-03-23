using Application.Common.Interfaces;
using Application.Clients.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Clients.Queries.ClientByIdQuery;

public class ClientByIdQueryHandler : IRequestHandler<ClientByIdQueryRequest, ClientByIdQueryResponse>
{
    private readonly IClientControlContext _context;

    public ClientByIdQueryHandler(IClientControlContext context)
    {
        _context = context;
    }

    public async Task<ClientByIdQueryResponse> Handle(ClientByIdQueryRequest request, CancellationToken cancellationToken)
    {
        return await _context.Clients
            .Where(x => x.Id == request.Id)
            .Select(x => new ClientByIdQueryResponse
            {
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                BirthDate = x.BirthDate,
                DocumentNumber = x.DocumentNumber,
                Address = new AddressModel
                {
                    PostalCode = x.Address.PostalCode,
                    AddressLine = x.Address.AddressLine,
                    Number = x.Address.Number,
                    Complement = x.Address.Complement,
                    Neighborhood = x.Address.Neighborhood,
                    City = x.Address.City,
                    State = x.Address.State
                }
            }).FirstOrDefaultAsync();
    }
}
