using MediatR;
using SlipStream.Common;
using SlipStream.Identity.Core.Contracts;

namespace SlipStream.Identity.Core.Users;

public sealed record GetMeQuery(Guid UserId) : IRequest<MeResponse>;

public class GetMeQueryHandler(IUserRepository users, IAccountService accounts) : IRequestHandler<GetMeQuery, MeResponse>
{
    public async Task<MeResponse> Handle(GetMeQuery request, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(request.UserId, ct)
                   ?? throw new NotFoundException("Користувача не знайдено.");

        var providers = await accounts.GetProvidersAsync(request.UserId, ct);
        return MeResponse.From(user, providers);
    }
}
