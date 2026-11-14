using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SlipStream.Identity.Infrastructure.Identity;

public static class GoogleAuthExtensions
{
    /// <summary>Тимчасова cookie, у якій живе результат входу через Google до моменту видачі наших JWT.</summary>
    public const string ExternalScheme = "External";

    public const string ProviderName = "Google";

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

                // Невдалий вхід (скасування, протухлий або відсутній state) дає не 500, а редірект на фронт.
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

        // ClientId і ClientSecret беруться із секції "Google" (user-secrets або Google__ClientId / Google__ClientSecret).
        services.AddOptions<GoogleOptions>(GoogleDefaults.AuthenticationScheme)
            .Bind(configuration.GetSection("Google"));

        return services;
    }
}
