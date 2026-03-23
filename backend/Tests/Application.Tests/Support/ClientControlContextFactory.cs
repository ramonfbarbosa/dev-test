using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Persistence;
using System;

namespace Application.Tests.Support;

internal static class ClientControlContextFactory
{
    public static ClientControlContext Create()
    {
        var options = new DbContextOptionsBuilder<ClientControlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new ClientControlContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        return context;
    }
}
