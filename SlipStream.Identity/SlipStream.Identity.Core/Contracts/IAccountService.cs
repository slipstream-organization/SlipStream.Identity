namespace SlipStream.Identity.Core.Contracts;

public sealed record AccountInfo(Guid Id, string UserName, string Email);

public sealed record ExternalLogin(string Provider, string ProviderKey, string Email, string DisplayName, string? AvatarUrl);

public interface IAccountService
{
    Task<(AccountInfo Account, bool IsNew)> FindOrCreateExternalAsync(ExternalLogin login, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetProvidersAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<AccountInfo?> FindByIdAsync(Guid id, CancellationToken ct = default);
}
