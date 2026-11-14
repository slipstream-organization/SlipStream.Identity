using Microsoft.AspNetCore.Identity;
using SlipStream.Common;
using SlipStream.Identity.Core.Contracts;

namespace SlipStream.Identity.Infrastructure.Identity;

internal sealed class AccountService(UserManager<ApplicationUser> userManager) : IAccountService
{
    public async Task<Guid> CreateAsync(string userName, string email, string password, CancellationToken ct = default)
    {
        var account = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = userName.Trim(),
            Email = email.Trim()
        };

        var result = await userManager.CreateAsync(account, password);
        if (!result.Succeeded)
            throw ToException(result.Errors);

        return account.Id;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var account = await userManager.FindByIdAsync(id.ToString());
        if (account is not null)
            await userManager.DeleteAsync(account);
    }

    public async Task<AccountInfo?> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default)
    {
        var account = await userManager.FindByEmailAsync(email.Trim());
        if (account is null)
            return null;

        if (await userManager.IsLockedOutAsync(account))
            return null;

        if (!await userManager.CheckPasswordAsync(account, password))
        {
            await userManager.AccessFailedAsync(account);
            return null;
        }

        await userManager.ResetAccessFailedCountAsync(account);
        return ToInfo(account);
    }

    public async Task<AccountInfo?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        var account = await userManager.FindByIdAsync(id.ToString());
        return account is null ? null : ToInfo(account);
    }

    private static AccountInfo ToInfo(ApplicationUser account) =>
        new(account.Id, account.UserName!, account.Email!);

    private static AppException ToException(IEnumerable<IdentityError> identityErrors)
    {
        var errors = identityErrors.ToList();

        if (errors.Any(e => e.Code == nameof(IdentityErrorDescriber.DuplicateUserName)))
            return new ConflictException("Це ім'я користувача вже зайняте.");

        if (errors.Any(e => e.Code == nameof(IdentityErrorDescriber.DuplicateEmail)))
            return new ConflictException("Цей email уже зареєстрований.");

        var grouped = errors
            .GroupBy(e => e.Code.StartsWith("Password") ? "password"
                        : e.Code.Contains("Email") ? "email"
                        : "userName")
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

        return new ValidationException(grouped);
    }
}
