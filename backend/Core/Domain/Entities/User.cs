using System;

namespace Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool Active { get; private set; } = false;
    public string EmailConfirmationTokenHash { get; set; }
    public DateTime? EmailConfirmationTokenExpiresAt { get; set; }
    public string PasswordHash { get; set; }
    public Profile Profile { get; set; }

    public User() { }

    public User(string username, string email, string passwordHash, Profile profile, bool emailConfirmed = false, bool active = false)
    {
        Username = username;
        Email = email;
        EmailConfirmed = emailConfirmed;
        Active = active;
        PasswordHash = passwordHash;
        Profile = profile;
    }

    public bool Update(string username, string email, Profile profile)
    {
        var emailChanged = !string.Equals(Email, email, StringComparison.OrdinalIgnoreCase);
        Username = username;
        Email = email;
        Profile = profile;
        if (emailChanged)
        {
            EmailConfirmed = false;
            Active = false;
            EmailConfirmationTokenHash = null;
            EmailConfirmationTokenExpiresAt = null;
        }
        return emailChanged;
    }

    public void SetEmailConfirmation(string tokenHash, DateTime expiresAt)
    {
        EmailConfirmed = false;
        EmailConfirmationTokenHash = tokenHash;
        EmailConfirmationTokenExpiresAt = expiresAt;
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        Active = true;
        EmailConfirmationTokenHash = null;
        EmailConfirmationTokenExpiresAt = null;
    }

    public void ToggleActive()
    {
        Active = !Active;
    }

}
