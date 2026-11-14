using Microsoft.EntityFrameworkCore;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Domain.Tokens;
using SlipStream.Identity.Infrastructure.Persistence;

namespace SlipStream.Identity.Infrastructure.Repositories;

internal sealed class RefreshTokenRepository(ApplicationDbContext db) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync(ct);
    }

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default) =>
        db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task DeleteAsync(RefreshToken token, CancellationToken ct = default)
    {
        db.RefreshTokens.Remove(token);
        await db.SaveChangesAsync(ct);
    }
}
