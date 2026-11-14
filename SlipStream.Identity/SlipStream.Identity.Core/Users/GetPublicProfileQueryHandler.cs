using MediatR;
using SlipStream.Common;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Domain.Users;

namespace SlipStream.Identity.Core.Users;

public sealed record GetPublicProfileQuery(string Handle) : IRequest<PublicUserResponse>;

public class GetPublicProfileQueryHandler(IUserRepository users) : IRequestHandler<GetPublicProfileQuery, PublicUserResponse>
{
    public async Task<PublicUserResponse> Handle(GetPublicProfileQuery request, CancellationToken ct)
    {
        var user = UserHandle.IsValid(request.Handle)
            ? await users.GetByHandleAsync(request.Handle, ct)
            : null;

        return user is null
            ? throw new NotFoundException("Користувача не знайдено.")
            : PublicUserResponse.From(user);
    }
}
