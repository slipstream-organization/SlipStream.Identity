using MediatR;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Core.Users;
using SlipStream.Identity.Domain.Users;

namespace SlipStream.Identity.Core.Sessions;

public sealed record ExternalLoginCommand(ExternalLogin Login) : IRequest<string>;

public class ExternalLoginCommandHandler : IRequestHandler<ExternalLoginCommand, string>
{
    private readonly IAccountService _accounts;
    private readonly IUserRepository _users;
    private readonly IAuthCodeStore _codes;

    public ExternalLoginCommandHandler(IAccountService accounts, IUserRepository users, IAuthCodeStore codes)
    {
        _accounts = accounts;
        _users = users;
        _codes = codes;
    }

    public async Task<string> Handle(ExternalLoginCommand request, CancellationToken ct)
    {
        var login = request.Login;
        var (account, isNew) = await _accounts.FindOrCreateExternalAsync(login, ct);

        if (isNew)
        {
            var handle = await HandleGenerator.GenerateAsync(_users, login.DisplayName, ct);
            await _users.CreateAsync(User.Create(account.Id, account.Email, handle, login.DisplayName, login.AvatarUrl), ct);
        }

        await _users.PatchAsync(account.Id, u =>
        {
            u.MarkLoggedIn();
            if (u.AvatarUrl is null)
            {
                u.SetAvatar(login.AvatarUrl);
            }
        }, ct);

        return await _codes.CreateAsync(account.Id, ct);
    }
}
