using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Domain.Users;

namespace SlipStream.Identity.UnitTests.Contracts;

internal sealed class FakeUserRepository : IUserRepository
{
    public FakeUserRepository()
    {
        Users = [];
    }

    public List<User> Users { get; }

    public User Add(string handle, string? email = null, string name = "Racer")
    {
        var user = User.Create(Guid.NewGuid(), email ?? $"{handle}@test.dev", handle, name);
        Users.Add(user);
        return user;
    }

    public Task CreateAsync(User user, CancellationToken ct = default)
    {
        Users.Add(user);
        return Task.CompletedTask;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult(Users.FirstOrDefault(u => u.Id == id));
    }

    public Task<User?> GetByHandleAsync(string handle, CancellationToken ct = default)
    {
        return Task.FromResult(Users.FirstOrDefault(u => u.NormalizedHandle == UserHandle.Normalize(handle)));
    }

    public Task<bool> IsHandleTakenAsync(string handle, Guid? excludeUserId = null, CancellationToken ct = default)
    {
        return Task.FromResult(Users.Any(u => u.NormalizedHandle == UserHandle.Normalize(handle) && u.Id != excludeUserId));
    }

    public Task<bool> PatchAsync(Guid id, Action<User> patch, CancellationToken ct = default)
    {
        var user = Users.FirstOrDefault(u => u.Id == id);
        if (user is null)
        {
            return Task.FromResult(false);
        }

        patch(user);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult(Users.RemoveAll(u => u.Id == id) > 0);
    }
}
