using MediatR;
using SlipStream.Identity.Core.Contracts;

namespace SlipStream.Identity.Core.Sessions;

public sealed record LogoutAllCommand(Guid UserId) : IRequest;

public class LogoutAllCommandHandler(IJwtProvider jwt) : IRequestHandler<LogoutAllCommand>
{
    public Task Handle(LogoutAllCommand request, CancellationToken ct) => jwt.RevokeAllAsync(request.UserId, ct);
}
