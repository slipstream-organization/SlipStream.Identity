using MediatR;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Core.Users;
using SlipStream.Identity.Domain.Users;

namespace SlipStream.Identity.Core.Sessions;

public sealed record ExternalLoginCommand(ExternalLogin Login) : IRequest<string>;

public class ExternalLoginCommandHandler(IAccountService accounts, IUserRepository users, IAuthCodeStore codes)
    : IRequestHandler<ExternalLoginCommand, string>
{
    public async Task<string> Handle(ExternalLoginCommand request, CancellationToken ct)
    {
        var login = request.Login;
        var (account, isNew) = await accounts.FindOrCreateExternalAsync(login, ct);

        if (isNew)
        {
            var handle = await HandleGenerator.GenerateAsync(users, login.DisplayName, ct);
            await users.CreateAsync(User.Create(account.Id, account.Email, handle, login.DisplayName, login.AvatarUrl), ct);
        }

        await users.PatchAsync(account.Id, u =>
        {
            u.MarkLoggedIn();
            if (u.AvatarUrl is null)
                u.SetAvatar(login.AvatarUrl);
        }, ct);

        return await codes.CreateAsync(account.Id, ct);
    }
}
