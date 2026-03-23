using Application.Users.Commands.ToggleActive;
using Application.Tests.Support;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;

namespace Application.Tests.Users.Commands;

public class ToggleUserActiveCommandTests
{
    [Fact]
    public async Task ValidateAsync_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        await using var context = ClientControlContextFactory.Create();
        var validator = new ToggleUserActiveCommandValidator(context);

        var result = await validator.ValidateAsync(new ToggleUserActiveCommandRequest { Id = Guid.NewGuid() });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(ToggleUserActiveCommandRequest.Id) &&
            error.ErrorMessage == "Usuário não encontrado.");
    }

    [Fact]
    public async Task Handle_WhenUserIsActive_DeactivatesUser()
    {
        await using var context = ClientControlContextFactory.Create();
        var user = TestUserFactory.Create(active: true);
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new ToggleUserActiveCommandHandler(context);

        await handler.Handle(new ToggleUserActiveCommandRequest { Id = user.Id }, CancellationToken.None);

        var persistedUser = await context.Users.SingleAsync(x => x.Id == user.Id);
        Assert.False(persistedUser.Active);
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ActivatesUser()
    {
        await using var context = ClientControlContextFactory.Create();
        var user = TestUserFactory.Create(active: false);
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new ToggleUserActiveCommandHandler(context);

        await handler.Handle(new ToggleUserActiveCommandRequest { Id = user.Id }, CancellationToken.None);

        var persistedUser = await context.Users.SingleAsync(x => x.Id == user.Id);
        Assert.True(persistedUser.Active);
    }
}
