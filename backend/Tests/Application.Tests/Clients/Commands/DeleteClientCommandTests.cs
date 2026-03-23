using Application.Clients.Commands.Delete;
using Application.Tests.Support;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;

namespace Application.Tests.Clients.Commands;

public class DeleteClientCommandTests
{
    [Fact]
    public async Task ValidateAsync_WhenClientDoesNotExist_ReturnsNotFoundError()
    {
        await using var context = ClientControlContextFactory.Create();
        var validator = new DeleteClientCommandValidator(context);

        var result = await validator.ValidateAsync(new DeleteClientCommandRequest { Id = Guid.NewGuid() });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(DeleteClientCommandRequest.Id) &&
            e.ErrorMessage == "Cliente não encontrado.");
    }

    [Fact]
    public async Task ValidateAsync_WhenClientExists_IsValid()
    {
        await using var context = ClientControlContextFactory.Create();
        var client = TestClientFactory.CreateEntity();
        context.Clients.Add(client);
        await context.SaveChangesAsync(CancellationToken.None);

        var validator = new DeleteClientCommandValidator(context);

        var result = await validator.ValidateAsync(new DeleteClientCommandRequest { Id = client.Id });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_WhenIdIsEmpty_ReturnsRequiredError()
    {
        await using var context = ClientControlContextFactory.Create();
        var validator = new DeleteClientCommandValidator(context);

        var result = await validator.ValidateAsync(new DeleteClientCommandRequest { Id = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(DeleteClientCommandRequest.Id) &&
            e.ErrorMessage == "Cliente é obrigatório.");
    }

    [Fact]
    public async Task Handle_WhenClientExists_RemovesClient()
    {
        await using var context = ClientControlContextFactory.Create();
        var client = TestClientFactory.CreateEntity();
        context.Clients.Add(client);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteClientCommandHandler(context);
        await handler.Handle(new DeleteClientCommandRequest { Id = client.Id }, CancellationToken.None);

        var exists = await context.Clients.AnyAsync(c => c.Id == client.Id);
        Assert.False(exists);
    }
}
