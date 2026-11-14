using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SlipStream.Identity.Infrastructure.Identity;

public static class GoogleAuthExtensions
{
    public const string ExternalScheme = "External";

    public const string ProviderName = "Google";

    public const string PictureClaim = "picture";

    internal static IServiceCollection AddGoogleLogin(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication()
            .AddCookie(ExternalScheme, o =>
            {
                o.Cookie.Name = "external_auth";
                o.ExpireTimeSpan = TimeSpan.FromMinutes(10);
            })
            .AddGoogle(o =>
            {
                o.SignInScheme = ExternalScheme;
                o.ClaimActions.MapJsonKey(PictureClaim, "picture");

                o.Events.OnRemoteFailure = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger(typeof(GoogleAuthExtensions));
                    logger.LogWarning(context.Failure, "Вхід через Google не вдався");

                    var frontendUrl = configuration["Frontend:Url"]?.TrimEnd('/') ?? "http://localhost:5173";
                    context.Response.Redirect($"{frontendUrl}/login?error=google_failed");
                    context.HandleResponse();
                    return Task.CompletedTask;
                };
            });

        services.AddOptions<GoogleOptions>(GoogleDefaults.AuthenticationScheme)
            .Bind(configuration.GetSection("Google"));

        return services;
    }
}
