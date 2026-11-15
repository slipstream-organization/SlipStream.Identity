using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using SlipStream.Common;

namespace SlipStream.Identity.Common;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        if (!Guid.TryParse(user.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id))
        {
            throw new UnauthorizedException("Недійсний токен доступу.");
        }

        return id;
    }
}
