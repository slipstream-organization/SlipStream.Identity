using MediatR;
using SlipStream.Identity.Core.Contracts;

namespace SlipStream.Identity.Core.Sessions;

public sealed record LogoutCommand : IRequest;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IJwtProvider _jwt;

    public LogoutCommandHandler(IJwtProvider jwt)
    {
        _jwt = jwt;
    }

    public Task Handle(LogoutCommand request, CancellationToken ct)
    {
        return _jwt.RevokeAsync(ct);
    }
}
