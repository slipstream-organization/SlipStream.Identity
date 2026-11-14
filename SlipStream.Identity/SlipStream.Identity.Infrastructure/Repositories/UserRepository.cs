using Microsoft.EntityFrameworkCore;
using Npgsql;
using SlipStream.Common;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Domain.Users;
using SlipStream.Identity.Infrastructure.Persistence;

namespace SlipStream.Identity.Infrastructure.Repositories;

internal sealed class UserRepository(ApplicationDbContext db) : IUserRepository
{
    public async Task CreateAsync(User user, CancellationToken ct = default)
    {
        db.Profiles.Add(user);
        await SaveAsync(ct);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Profiles.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByHandleAsync(string handle, CancellationToken ct = default)
    {
        var normalized = UserHandle.Normalize(handle);
        return db.Profiles.AsNoTracking().FirstOrDefaultAsync(u => u.NormalizedHandle == normalized, ct);
    }

    public Task<bool> IsHandleTakenAsync(string handle, Guid? excludeUserId = null, CancellationToken ct = default)
    {
        var normalized = UserHandle.Normalize(handle);
        return db.Profiles.AnyAsync(u => u.NormalizedHandle == normalized && u.Id != excludeUserId, ct);
    }

    public async Task<bool> PatchAsync(Guid id, Action<User> patch, CancellationToken ct = default)
    {
        var user = await db.Profiles.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return false;

        patch(user);
        await SaveAsync(ct);
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

    private async Task SaveAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg
                                           && pg.ConstraintName?.Contains(nameof(User.NormalizedHandle)) == true)
        {
            throw new ConflictException("Цей нік уже зайнятий.");
        }
    }
}
