using MediatR;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Domain.Users;

namespace SlipStream.Identity.Core.Sessions;

public sealed record ExternalLoginCommand(ExternalLogin Login) : IRequest<TokenResponse>;

public class ExternalLoginCommandHandler(IAccountService accounts, IUserRepository users, IJwtProvider jwt)
    : IRequestHandler<ExternalLoginCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(ExternalLoginCommand request, CancellationToken ct)
    {
        var (account, isNew) = await accounts.FindOrCreateExternalAsync(request.Login, ct);

        if (isNew)
            await users.CreateAsync(User.Create(account.Id, account.Email, request.Login.DisplayName), ct);

        await users.PatchAsync(account.Id, u => u.MarkLoggedIn(), ct);

        return await jwt.IssueAsync(account, ct);
    }
}
