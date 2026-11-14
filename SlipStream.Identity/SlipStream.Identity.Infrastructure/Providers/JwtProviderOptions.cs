using System.ComponentModel.DataAnnotations;
using SlipStream.Common.Authentication;

namespace SlipStream.Identity.Infrastructure.Providers;

public sealed class JwtProviderOptions : JwtOptions
{
    [Range(1, 1440)]
    public int AccessTokenMinutes { get; set; } = 15;

    [Range(1, 365)]
    public int RefreshTokenDays { get; set; } = 30;
}
