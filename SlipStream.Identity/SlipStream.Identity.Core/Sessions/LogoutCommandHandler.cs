using MediatR;
using SlipStream.Identity.Core.Contracts;

namespace SlipStream.Identity.Core.Sessions;

public sealed record LogoutCommand : IRequest;

public class LogoutCommandHandler(IJwtProvider jwt) : IRequestHandler<LogoutCommand>
{
    public Task Handle(LogoutCommand request, CancellationToken ct) => jwt.RevokeAsync(ct);
}
