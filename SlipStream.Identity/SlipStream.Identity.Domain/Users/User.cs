namespace SlipStream.Identity.Domain.Users;

public sealed class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string DisplayName { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }

    private User()
    {
    }

    public static User Create(Guid id, string email, string displayName)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Ідентифікатор користувача обов'язковий.", nameof(id));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email обов'язковий.", nameof(email));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Ім'я користувача обов'язкове.", nameof(displayName));

        return new User
        {
            Id = id,
            Email = email.Trim().ToLowerInvariant(),
            DisplayName = displayName.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Rename(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Ім'я користувача обов'язкове.", nameof(displayName));

        DisplayName = displayName.Trim();
    }

    public void MarkLoggedIn() => LastLoginAtUtc = DateTime.UtcNow;
}
