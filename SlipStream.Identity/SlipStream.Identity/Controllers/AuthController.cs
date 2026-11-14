using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SlipStream.Common;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Core.Sessions;
using SlipStream.Identity.Infrastructure.Identity;
using SlipStream.Identity.Options;

namespace SlipStream.Identity.Controllers;

[ApiController]
[Route("v1/auth")]
public class AuthController(IMediator mediator, IOptions<FrontendOptions> frontend) : ControllerBase
{
    [HttpGet("google")]
    public IActionResult Google([FromQuery] string? returnUrl)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback), new { returnUrl })
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }
    
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string? returnUrl)
    {
        var result = await HttpContext.AuthenticateAsync(GoogleAuthExtensions.ExternalScheme);
        await HttpContext.SignOutAsync(GoogleAuthExtensions.ExternalScheme);

        if (!result.Succeeded)
            return RedirectToFrontend("/login", "google_failed");

        var principal = result.Principal;
        var key = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal.FindFirstValue(ClaimTypes.Email);
        var name = principal.FindFirstValue(ClaimTypes.Name) ?? email;

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(email))
            return RedirectToFrontend("/login", "google_no_email");

        try
        {
            await mediator.Send(new ExternalLoginCommand(
                new ExternalLogin(GoogleAuthExtensions.ProviderName, key, email, name!)));
        }
        catch (AppException)
        {
            return RedirectToFrontend("/login", "google_failed");
        }

        return RedirectToFrontend(returnUrl);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponse>> Refresh()
    {
        return Ok(await mediator.Send(new RefreshCommand()));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await mediator.Send(new LogoutCommand());
        return NoContent();
    }
    
    private RedirectResult RedirectToFrontend(string? path, string? error = null)
    {
        var safePath = !string.IsNullOrEmpty(path) && path.StartsWith('/') && !path.StartsWith("//") && !path.StartsWith("/\\")
            ? path
            : "/";

        var url = frontend.Value.Url.TrimEnd('/') + safePath;
        if (error is not null)
            url += (url.Contains('?') ? "&" : "?") + "error=" + Uri.EscapeDataString(error);

        return Redirect(url);
    }
}
