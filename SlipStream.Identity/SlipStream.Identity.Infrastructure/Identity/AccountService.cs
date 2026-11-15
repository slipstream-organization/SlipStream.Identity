using Microsoft.AspNetCore.Identity;
using SlipStream.Common;
using SlipStream.Identity.Core.Contracts;

namespace SlipStream.Identity.Infrastructure.Identity;

internal sealed class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(AccountInfo Account, bool IsNew)> FindOrCreateExternalAsync(
        ExternalLogin login, CancellationToken ct = default)
    {
        var account = await _userManager.FindByLoginAsync(login.Provider, login.ProviderKey);
        if (account is not null)
        {
            return (ToInfo(account), false);
        }

        account = await _userManager.FindByEmailAsync(login.Email);
        var isNew = account is null;

        if (account is null)
        {
            account = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = $"{login.Provider}_{login.ProviderKey}".ToLowerInvariant(),
                Email = login.Email,
                EmailConfirmed = true
            };
            EnsureSucceeded(await _userManager.CreateAsync(account));
        }

        EnsureSucceeded(await _userManager.AddLoginAsync(
            account, new UserLoginInfo(login.Provider, login.ProviderKey, login.Provider)));

        return (ToInfo(account), isNew);
    }

    public async Task<IReadOnlyList<string>> GetProvidersAsync(Guid id, CancellationToken ct = default)
    {
        var account = await _userManager.FindByIdAsync(id.ToString());
        if (account is null)
        {
            return [];
        }

        var logins = await _userManager.GetLoginsAsync(account);
        return logins.Select(l => l.LoginProvider.ToLowerInvariant()).Distinct().ToList();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var account = await _userManager.FindByIdAsync(id.ToString());
        if (account is not null)
        {
            await _userManager.DeleteAsync(account);
        }
    }

    public async Task<AccountInfo?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        var account = await _userManager.FindByIdAsync(id.ToString());
        if (account is null)
        {
            return null;
        }

        return ToInfo(account);
    }

    private static AccountInfo ToInfo(ApplicationUser account)
    {
        return new AccountInfo(account.Id, account.UserName!, account.Email!);
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }
}
