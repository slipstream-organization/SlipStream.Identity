using Microsoft.AspNetCore.Identity;
using SlipStream.Common;
using SlipStream.Identity.Core.Contracts;

namespace SlipStream.Identity.Infrastructure.Identity;

internal sealed class AccountService(UserManager<ApplicationUser> userManager) : IAccountService
{
    public async Task<(AccountInfo Account, bool IsNew)> FindOrCreateExternalAsync(
        ExternalLogin login, CancellationToken ct = default)
    {
        var account = await userManager.FindByLoginAsync(login.Provider, login.ProviderKey);
        if (account is not null)
            return (ToInfo(account), false);

        // Google віддає лише підтверджені email, тож акаунт із таким email можна безпечно прив'язати.
        account = await userManager.FindByEmailAsync(login.Email);
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
            EnsureSucceeded(await userManager.CreateAsync(account));
        }

        EnsureSucceeded(await userManager.AddLoginAsync(
            account, new UserLoginInfo(login.Provider, login.ProviderKey, login.Provider)));

        return (ToInfo(account), isNew);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var account = await userManager.FindByIdAsync(id.ToString());
        if (account is not null)
            await userManager.DeleteAsync(account);
    }

    public async Task<AccountInfo?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        var account = await userManager.FindByIdAsync(id.ToString());
        return account is null ? null : ToInfo(account);
    }

    private static AccountInfo ToInfo(ApplicationUser account) =>
        new(account.Id, account.UserName!, account.Email!);

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));
    }
}
