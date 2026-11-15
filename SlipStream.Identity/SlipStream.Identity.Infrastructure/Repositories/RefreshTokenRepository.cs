using Microsoft.EntityFrameworkCore;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Domain.Tokens;
using SlipStream.Identity.Infrastructure.Persistence;

namespace SlipStream.Identity.Infrastructure.Repositories;

internal sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _db;

    public RefreshTokenRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync(ct);
    }

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default)
    {
        return _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);
    }

    public async Task DeleteAsync(RefreshToken token, CancellationToken ct = default)
    {
        _db.RefreshTokens.Remove(token);
        await _db.SaveChangesAsync(ct);
    }

    public Task DeleteAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return _db.RefreshTokens.Where(t => t.UserId == userId).ExecuteDeleteAsync(ct);
    }
}
