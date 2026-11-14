namespace SlipStream.Identity.Core.Contracts;

public sealed record TokenResponse(string AccessToken, DateTime ExpiresAtUtc);

public interface IJwtProvider
{
    Task<TokenResponse> IssueAsync(AccountInfo account, CancellationToken ct = default);
    Task<TokenResponse> RefreshAsync(CancellationToken ct = default);
    Task RevokeAsync(CancellationToken ct = default);

    Task RevokeAllAsync(Guid userId, CancellationToken ct = default);
}
