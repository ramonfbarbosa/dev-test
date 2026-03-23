using Domain;
using Domain.Entities;

namespace Application.Tests.Domain;

public class UserActivationTests
{
    [Fact]
    public void Deactivate_WhenUserIsActive_SetsActiveToFalse()
    {
        var user = new User("active_user", "active_user@clientcontrol.local", "hash", Profile.Operator, true, true);

        user.ToggleActive();

        Assert.False(user.Active);
    }

    [Fact]
    public void Activate_WhenUserIsInactive_SetsActiveToTrue()
    {
        var user = new User("inactive_user", "inactive_user@clientcontrol.local", "hash", Profile.Operator, true, false);

        user.ToggleActive();

        Assert.True(user.Active);
    }
}
