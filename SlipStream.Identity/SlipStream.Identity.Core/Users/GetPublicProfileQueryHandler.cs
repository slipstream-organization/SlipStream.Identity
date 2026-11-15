using MediatR;
using SlipStream.Common;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Domain.Users;

namespace SlipStream.Identity.Core.Users;

public sealed record GetPublicProfileQuery(string Handle) : IRequest<PublicUserResponse>;

public class GetPublicProfileQueryHandler : IRequestHandler<GetPublicProfileQuery, PublicUserResponse>
{
    private readonly IUserRepository _users;

    public GetPublicProfileQueryHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<PublicUserResponse> Handle(GetPublicProfileQuery request, CancellationToken ct)
    {
        if (!UserHandle.IsValid(request.Handle))
        {
            throw new NotFoundException("Користувача не знайдено.");
        }

        var user = await _users.GetByHandleAsync(request.Handle, ct);
        if (user is null)
        {
            throw new NotFoundException("Користувача не знайдено.");
        }

        return PublicUserResponse.From(user);
    }
}
