using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using SlipStream.Common;

namespace SlipStream.Identity.Common;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id)
            ? id
            : throw new UnauthorizedException("Недійсний токен доступу.");
}
