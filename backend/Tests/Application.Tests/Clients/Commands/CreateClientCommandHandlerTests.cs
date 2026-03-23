using Application.Clients.Commands.Create;
using Application.Tests.Support;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Application.Tests.Clients.Commands;

public class CreateClientCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenRequestIsValid_PersistsClientAndReturnsId()
    {
        await using var context = ClientControlContextFactory.Create();
        var handler = new CreateClientCommandHandler(context);
        var request = TestClientFactory.CreateRequest();

        var id = await handler.Handle(request, CancellationToken.None);

        var persisted = await context.Clients
            .Include(c => c.Address)
            .SingleAsync(c => c.Id == id, CancellationToken.None);

        Assert.Equal(request.FirstName, persisted.FirstName);
        Assert.Equal(request.LastName, persisted.LastName);
        Assert.Equal(request.Email, persisted.Email);
        Assert.Equal(request.BirthDate, persisted.BirthDate);
        Assert.NotNull(persisted.Address);
        Assert.Equal(request.Address.City, persisted.Address.City);
        Assert.Equal(request.Address.State, persisted.Address.State);
    }
}
