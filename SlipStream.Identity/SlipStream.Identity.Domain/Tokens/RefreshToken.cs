namespace SlipStream.Identity.Domain.Tokens;

/// <summary>Refresh-токен. У БД зберігається лише SHA-256 хеш, сам токен живе тільки в cookie клієнта.</summary>
public sealed class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    private RefreshToken()
    {
    }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAtUtc) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        TokenHash = tokenHash,
        CreatedAtUtc = DateTime.UtcNow,
        ExpiresAtUtc = expiresAtUtc
    };

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
}
