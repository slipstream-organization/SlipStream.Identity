using SlipStream.Identity.Domain.Tokens;

namespace SlipStream.Identity.Core.Contracts;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);
    Task DeleteAsync(RefreshToken token, CancellationToken ct = default);
}
