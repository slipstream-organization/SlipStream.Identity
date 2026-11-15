using SlipStream.Identity.Core.Contracts;

namespace SlipStream.Identity.UnitTests.Contracts;

internal sealed class FakeAccountService : IAccountService
{
    public FakeAccountService()
    {
        Accounts = [];
        Logins = [];
    }

    public Dictionary<Guid, AccountInfo> Accounts { get; }
    public Dictionary<(string Provider, string Key), Guid> Logins { get; }

    public AccountInfo Add(Guid id, string email, string? googleKey = null)
    {
        var account = new AccountInfo(id, $"user_{id:N}", email);
        Accounts[id] = account;
        if (googleKey is not null)
        {
            Logins[("Google", googleKey)] = id;
        }

        return account;
    }

    public Task<(AccountInfo Account, bool IsNew)> FindOrCreateExternalAsync(ExternalLogin login, CancellationToken ct = default)
    {
        if (Logins.TryGetValue((login.Provider, login.ProviderKey), out var linkedId))
        {
            return Task.FromResult((Accounts[linkedId], false));
        }

        var byEmail = Accounts.Values.FirstOrDefault(a => a.Email == login.Email);
        if (byEmail is not null)
        {
            Logins[(login.Provider, login.ProviderKey)] = byEmail.Id;
            return Task.FromResult((byEmail, false));
        }

        var created = Add(Guid.NewGuid(), login.Email);
        Logins[(login.Provider, login.ProviderKey)] = created.Id;
        return Task.FromResult((created, true));
    }

    public Task<IReadOnlyList<string>> GetProvidersAsync(Guid id, CancellationToken ct = default)
    {
        IReadOnlyList<string> providers = Logins
            .Where(l => l.Value == id)
            .Select(l => l.Key.Provider.ToLowerInvariant())
            .Distinct()
            .ToList();

        return Task.FromResult(providers);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        Accounts.Remove(id);
        return Task.CompletedTask;
    }

    public Task<AccountInfo?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult(Accounts.GetValueOrDefault(id));
    }
}
