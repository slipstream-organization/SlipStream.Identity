using MediatR;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Domain.Users;

namespace SlipStream.Identity.Core.Users;

public sealed record CheckHandleQuery(string? Handle) : IRequest<HandleAvailabilityResponse>;

public class CheckHandleQueryHandler : IRequestHandler<CheckHandleQuery, HandleAvailabilityResponse>
{
    private readonly IUserRepository _users;

    public CheckHandleQueryHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<HandleAvailabilityResponse> Handle(CheckHandleQuery request, CancellationToken ct)
    {
        if (!UserHandle.IsValid(request.Handle))
        {
            return new HandleAvailabilityResponse(false);
        }

        return new HandleAvailabilityResponse(!await _users.IsHandleTakenAsync(request.Handle!, ct: ct));
    }
}
