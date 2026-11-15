namespace SlipStream.Identity.Domain.Users;

public sealed class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Handle { get; private set; } = string.Empty;
    public string NormalizedHandle { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public string? FavouriteTeam { get; private set; }
    public string? FavouriteDriver { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }

    private User()
    {
    }

    private User(Guid id, string email, string handle, string displayName, string? avatarUrl)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Ідентифікатор користувача обов'язковий.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email обов'язковий.", nameof(email));
        }

        Id = id;
        Email = email.Trim().ToLowerInvariant();
        CreatedAtUtc = DateTime.UtcNow;
        ChangeHandle(handle);
        Rename(displayName);
        SetAvatar(avatarUrl);
    }

    public static User Create(Guid id, string email, string handle, string displayName, string? avatarUrl = null)
    {
        return new User(id, email, handle, displayName, avatarUrl);
    }

    public void ChangeHandle(string handle)
    {
        if (!UserHandle.IsValid(handle))
        {
            throw new ArgumentException(UserHandle.FormatMessage, nameof(handle));
        }

        Handle = handle;
        NormalizedHandle = UserHandle.Normalize(handle);
    }

    public void Rename(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Ім'я користувача обов'язкове.", nameof(displayName));
        }

        DisplayName = displayName.Trim();
    }

    public void SetAvatar(string? avatarUrl)
    {
        AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
    }

    public void SetFavouriteTeam(string? teamId)
    {
        FavouriteTeam = string.IsNullOrWhiteSpace(teamId) ? null : teamId;
    }

    public void SetFavouriteDriver(string? driverCode)
    {
        FavouriteDriver = string.IsNullOrWhiteSpace(driverCode) ? null : driverCode;
    }

    public void MarkLoggedIn()
    {
        LastLoginAtUtc = DateTime.UtcNow;
    }
}
