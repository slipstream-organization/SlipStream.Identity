using MediatR;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Domain.Users;

namespace SlipStream.Identity.Core.Users;

public sealed record CheckHandleQuery(string? Handle) : IRequest<HandleAvailabilityResponse>;

public class CheckHandleQueryHandler(IUserRepository users) : IRequestHandler<CheckHandleQuery, HandleAvailabilityResponse>
{
    public async Task<HandleAvailabilityResponse> Handle(CheckHandleQuery request, CancellationToken ct)
    {
        if (!UserHandle.IsValid(request.Handle))
            return new HandleAvailabilityResponse(false);

        return new HandleAvailabilityResponse(!await users.IsHandleTakenAsync(request.Handle!, ct: ct));
    }
}
