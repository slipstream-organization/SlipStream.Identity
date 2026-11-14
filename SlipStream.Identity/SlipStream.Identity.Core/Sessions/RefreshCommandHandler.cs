using MediatR;
using SlipStream.Identity.Core.Contracts;

namespace SlipStream.Identity.Core.Sessions;

public sealed record RefreshCommand : IRequest<TokenResponse>;

public class RefreshCommandHandler(IJwtProvider jwt) : IRequestHandler<RefreshCommand, TokenResponse>
{
    public Task<TokenResponse> Handle(RefreshCommand request, CancellationToken ct) => jwt.RefreshAsync(ct);
}
