using SlipStream.Identity.Core.Contracts;

namespace SlipStream.Identity.UnitTests.Contracts;

internal sealed class FakeJwtProvider : IJwtProvider
{
    public FakeJwtProvider()
    {
        Token = new TokenResponse("access-token", DateTime.UtcNow.AddMinutes(15));
        IssuedFor = [];
        RevokedAllFor = [];
    }

    public TokenResponse Token { get; }
    public List<Guid> IssuedFor { get; }
    public int RefreshCalls { get; private set; }
    public int RevokeCalls { get; private set; }
    public List<Guid> RevokedAllFor { get; }

    public Task<TokenResponse> IssueAsync(AccountInfo account, CancellationToken ct = default)
    {
        IssuedFor.Add(account.Id);
        return Task.FromResult(Token);
    }

    public Task<TokenResponse> RefreshAsync(CancellationToken ct = default)
    {
        RefreshCalls++;
        return Task.FromResult(Token);
    }

    public Task RevokeAsync(CancellationToken ct = default)
    {
        RevokeCalls++;
        return Task.CompletedTask;
    }

    public Task RevokeAllAsync(Guid userId, CancellationToken ct = default)
    {
        RevokedAllFor.Add(userId);
        return Task.CompletedTask;
    }
}
