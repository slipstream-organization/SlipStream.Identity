using SlipStream.Identity.Domain.Users;

namespace SlipStream.Identity.Core.Contracts;

public interface IUserRepository
{
    Task CreateAsync(User user, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> PatchAsync(Guid id, Action<User> patch, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
