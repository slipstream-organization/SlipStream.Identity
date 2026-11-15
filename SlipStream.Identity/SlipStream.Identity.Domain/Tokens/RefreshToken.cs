namespace SlipStream.Identity.Domain.Tokens;

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

    private RefreshToken(Guid userId, string tokenHash, DateTime expiresAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAtUtc)
    {
        return new RefreshToken(userId, tokenHash, expiresAtUtc);
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
}
