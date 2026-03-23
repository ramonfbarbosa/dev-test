using Application.Common.Interfaces;
using Application.Common.Models;
using Common;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Clients.Queries.FilteredClientsQuery;

public class FilteredClientsQueryHandler : IRequestHandler<FilteredClientsQueryRequest, PagedList<FilteredClientsQueryResponse>>
{
    private readonly IClientControlContext _context;

    public FilteredClientsQueryHandler(IClientControlContext context)
    {
        _context = context;
    }

    public async Task<PagedList<FilteredClientsQueryResponse>> Handle(FilteredClientsQueryRequest request, CancellationToken cancellationToken)
    {
        var filteredClients = ApplyFilters(_context.Clients.AsEnumerable(), request);
        var projectedClients = ProjectToResponse(filteredClients);
        var sortedClients = ApplySorting(projectedClients, request.SortBy, request.SortDirection);
        return PagedList<FilteredClientsQueryResponse>.Create(sortedClients, request.Page, request.PageSize);
    }

    private static IEnumerable<Client> ApplyFilters(IEnumerable<Client> query, FilteredClientsQueryRequest request)
    {
        if (!string.IsNullOrEmpty(request.DocumentNumber))
        {
            var unmaskedFilter = request.DocumentNumber.UnMask();
            query = query.Where(c =>
                c.DocumentNumber != null &&
                c.DocumentNumber.UnMask().Contains(unmaskedFilter));
        }
        return query;
    }

    private static List<FilteredClientsQueryResponse> ProjectToResponse(IEnumerable<Client> clients)
    {
        return clients
            .Select(c => new FilteredClientsQueryResponse
            {
                Id = c.Id,
                CreatedAt = c.CreatedAt,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                DocumentNumber = c.DocumentNumber,
                BirthDate = c.BirthDate,
                Address = new Models.AddressModel
                {
                    PostalCode = c.Address.PostalCode,
                    AddressLine = c.Address.AddressLine,
                    Number = c.Address.Number,
                    Complement = c.Address.Complement,
                    Neighborhood = c.Address.Neighborhood,
                    City = c.Address.City,
                    State = c.Address.State
                }
            })
            .ToList();
    }

    private static IEnumerable<FilteredClientsQueryResponse> ApplySorting(
        List<FilteredClientsQueryResponse> clients, string sortBy, string sortDirection)
    {
        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return sortBy?.ToLower() switch
        {
            "lastname" => isDescending ? clients.OrderByDescending(c => c.LastName) : clients.OrderBy(c => c.LastName),
            "email" => isDescending ? clients.OrderByDescending(c => c.Email) : clients.OrderBy(c => c.Email),
            "birthdate" => isDescending ? clients.OrderByDescending(c => c.BirthDate) : clients.OrderBy(c => c.BirthDate),
            "phonenumber" => isDescending ? clients.OrderByDescending(c => c.PhoneNumber) : clients.OrderBy(c => c.PhoneNumber),
            "documentnumber" => isDescending ? clients.OrderByDescending(c => c.DocumentNumber) : clients.OrderBy(c => c.DocumentNumber),
            _ => isDescending
                ? clients.OrderByDescending(c => c.FirstName).ThenByDescending(c => c.LastName)
                : clients.OrderBy(c => c.FirstName).ThenBy(c => c.LastName),
        };
    }
}
