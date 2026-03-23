using Domain;
using Domain.Entities;
using System;

namespace Application.Tests.Support;

internal static class TestUserFactory
{
    public const string DefaultPassword = "Temp123!";

    public static User Create(
        bool active = true,
        bool emailConfirmed = true,
        Profile profile = Profile.Operator)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        return new User(
            $"user_{suffix}",
            $"user_{suffix}@clientcontrol.local",
            BCrypt.Net.BCrypt.HashPassword(DefaultPassword),
            profile,
            emailConfirmed,
            active);
    }
}
