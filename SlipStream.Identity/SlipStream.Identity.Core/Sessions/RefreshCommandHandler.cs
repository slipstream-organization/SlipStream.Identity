using MediatR;
using SlipStream.Identity.Core.Contracts;

namespace SlipStream.Identity.Core.Sessions;

public sealed record RefreshCommand : IRequest<TokenResponse>;

public class RefreshCommandHandler : IRequestHandler<RefreshCommand, TokenResponse>
{
    private readonly IJwtProvider _jwt;

    public RefreshCommandHandler(IJwtProvider jwt)
    {
        _jwt = jwt;
    }

    public Task<TokenResponse> Handle(RefreshCommand request, CancellationToken ct)
    {
        return _jwt.RefreshAsync(ct);
    }
}
