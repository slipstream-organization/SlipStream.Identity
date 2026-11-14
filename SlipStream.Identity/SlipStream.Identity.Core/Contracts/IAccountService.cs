namespace SlipStream.Identity.Core.Contracts;

public sealed record AccountInfo(Guid Id, string UserName, string Email);

public interface IAccountService
{
    Task<Guid> CreateAsync(string userName, string email, string password, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<AccountInfo?> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default);
    Task<AccountInfo?> FindByIdAsync(Guid id, CancellationToken ct = default);
}
