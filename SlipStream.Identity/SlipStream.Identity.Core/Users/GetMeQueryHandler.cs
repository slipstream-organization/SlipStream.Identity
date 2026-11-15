using MediatR;
using SlipStream.Common;
using SlipStream.Identity.Core.Contracts;

namespace SlipStream.Identity.Core.Users;

public sealed record GetMeQuery(Guid UserId) : IRequest<MeResponse>;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, MeResponse>
{
    private readonly IUserRepository _users;
    private readonly IAccountService _accounts;

    public GetMeQueryHandler(IUserRepository users, IAccountService accounts)
    {
        _users = users;
        _accounts = accounts;
    }

    public async Task<MeResponse> Handle(GetMeQuery request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            throw new NotFoundException("Користувача не знайдено.");
        }

        var providers = await _accounts.GetProvidersAsync(request.UserId, ct);
        return MeResponse.From(user, providers);
    }
}
