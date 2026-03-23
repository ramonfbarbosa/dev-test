using Application.Clients.Commands.Update;
using Application.Clients.Models;
using Application.Tests.Support;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;

namespace Application.Tests.Clients.Commands;

public class UpdateClientCommandTests
{
    [Fact]
    public async Task ValidateAsync_WhenDocumentIsDuplicateForDifferentClient_ReturnsError()
    {
        await using var context = ClientControlContextFactory.Create();
        var existing = TestClientFactory.CreateEntity(documentNumber: "99988877766");
        var toUpdate = TestClientFactory.CreateEntity(documentNumber: "11122233344");
        context.Clients.AddRange(existing, toUpdate);
        await context.SaveChangesAsync(CancellationToken.None);

        var validator = new UpdateClientCommandValidator(context);
        var request = CreateValidRequest(toUpdate.Id);
        request.DocumentNumber = "99988877766";

        var result = await validator.ValidateAsync(request, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(UpdateClientCommandRequest.DocumentNumber) &&
            e.ErrorMessage == "Já existe um cliente com o documento informado.");
    }

    [Fact]
    public async Task ValidateAsync_WhenSameClientKeepsSameDocument_IsValid()
    {
        await using var context = ClientControlContextFactory.Create();
        var client = TestClientFactory.CreateEntity(documentNumber: "55566677788");
        context.Clients.Add(client);
        await context.SaveChangesAsync(CancellationToken.None);

        var validator = new UpdateClientCommandValidator(context);
        var request = CreateValidRequest(client.Id);
        request.DocumentNumber = "55566677788";

        var result = await validator.ValidateAsync(request, CancellationToken.None);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_UpdatesClientFields()
    {
        await using var context = ClientControlContextFactory.Create();
        var client = TestClientFactory.CreateEntity();
        context.Clients.Add(client);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateClientCommandHandler(context);
        var request = CreateValidRequest(client.Id);
        request.FirstName = "NovoNome";
        request.Email = "novo@test.local";

        await handler.Handle(request, CancellationToken.None);

        var persisted = await context.Clients
            .Include(c => c.Address)
            .SingleAsync(c => c.Id == client.Id, CancellationToken.None);

        Assert.Equal("NovoNome", persisted.FirstName);
        Assert.Equal("novo@test.local", persisted.Email);
    }

    [Fact]
    public async Task ValidateAsync_WhenIdIsEmpty_ReturnsError()
    {
        await using var context = ClientControlContextFactory.Create();
        var validator = new UpdateClientCommandValidator(context);
        var request = CreateValidRequest(Guid.Empty);

        var result = await validator.ValidateAsync(request, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(UpdateClientCommandRequest.Id) &&
            e.ErrorMessage == "Cliente é obrigatório");
    }

    private static UpdateClientCommandRequest CreateValidRequest(Guid id) => new()
    {
        Id = id,
        FirstName = "Teste",
        LastName = "Sobrenome",
        PhoneNumber = "11999990000",
        Email = "update@test.local",
        DocumentNumber = "00011122233",
        BirthDate = new DateTime(1995, 6, 20),
        Address = new AddressModel
        {
            PostalCode = "01001000",
            AddressLine = "Rua Atualizada",
            Number = "200",
            Complement = "Apto 1",
            Neighborhood = "Centro",
            City = "Rio de Janeiro",
            State = "RJ"
        }
    };
}
