using Application.Clients.Commands.Create;
using Application.Clients.Models;
using Domain.Entities;
using System;

namespace Application.Tests.Support;

internal static class TestClientFactory
{
    public static Client CreateEntity(string documentNumber = null)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return new Client(
            $"First_{suffix}",
            $"Last_{suffix}",
            "11999990000",
            $"client_{suffix}@test.local",
            documentNumber ?? suffix,
            new DateTime(1990, 1, 15),
            new Address("01001000", "Rua Teste", "100", null, "Centro", "São Paulo", "SP"));
    }

    public static CreateClientCommandRequest CreateRequest(string documentNumber = null)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return new CreateClientCommandRequest
        {
            FirstName = $"First_{suffix}",
            LastName = $"Last_{suffix}",
            PhoneNumber = "11999990000",
            Email = $"client_{suffix}@test.local",
            DocumentNumber = documentNumber ?? suffix,
            BirthDate = new DateTime(1990, 1, 15),
            Address = new AddressModel
            {
                PostalCode = "01001000",
                AddressLine = "Rua Teste",
                Number = "100",
                Complement = null,
                Neighborhood = "Centro",
                City = "São Paulo",
                State = "SP"
            }
        };
    }
}
