using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SlipStream.Common.Authentication;
using SlipStream.Identity.Core.Contracts;

namespace SlipStream.Identity.Infrastructure.Providers;

internal static class JwtProviderExtensions
{
    public static IServiceCollection AddJwt(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddJwtAuthentication(configuration);

        services.AddOptions<JwtProviderOptions>()
            .Bind(configuration.GetSection(JwtOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpContextAccessor();
        services.AddScoped<IJwtProvider, JwtProvider>();

        return services;
    }
}
