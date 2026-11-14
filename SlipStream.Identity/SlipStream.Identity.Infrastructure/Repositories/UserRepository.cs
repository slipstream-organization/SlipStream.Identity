using Microsoft.EntityFrameworkCore;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Domain.Users;
using SlipStream.Identity.Infrastructure.Persistence;

namespace SlipStream.Identity.Infrastructure.Repositories;

internal sealed class UserRepository(ApplicationDbContext db) : IUserRepository
{
    public async Task CreateAsync(User user, CancellationToken ct = default)
    {
        db.Profiles.Add(user);
        await db.SaveChangesAsync(ct);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return db.Profiles.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<bool> PatchAsync(Guid id, Action<User> patch, CancellationToken ct = default)
    {
        var user = await db.Profiles.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return false;

        patch(user);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await db.Profiles.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return false;

        db.Profiles.Remove(user);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
