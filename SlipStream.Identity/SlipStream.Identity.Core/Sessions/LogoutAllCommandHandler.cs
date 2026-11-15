using MediatR;
using SlipStream.Identity.Core.Contracts;

namespace SlipStream.Identity.Core.Sessions;

public sealed record LogoutAllCommand(Guid UserId) : IRequest;

public class LogoutAllCommandHandler : IRequestHandler<LogoutAllCommand>
{
    private readonly IJwtProvider _jwt;

    public LogoutAllCommandHandler(IJwtProvider jwt)
    {
        _jwt = jwt;
    }

    public Task Handle(LogoutAllCommand request, CancellationToken ct)
    {
        return _jwt.RevokeAllAsync(request.UserId, ct);
    }
}
