using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SlipStream.Common;
using SlipStream.Identity.Common;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Core.Sessions;
using SlipStream.Identity.Infrastructure.Identity;
using SlipStream.Identity.Options;

namespace SlipStream.Identity.Controllers;

[ApiController]
[Route("v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly FrontendOptions _frontend;

    public AuthController(IMediator mediator, IOptions<FrontendOptions> frontend)
    {
        _mediator = mediator;
        _frontend = frontend.Value;
    }

    [HttpGet("google")]
    public IActionResult Google([FromQuery] string? returnUrl)
    {
        if (returnUrl is not null && !ReturnUrl.IsValid(returnUrl))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["returnUrl"] = ["returnUrl має бути відносним шляхом, наприклад /dashboard."]
            });
        }

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
        {
            return RedirectToFrontend("/login", new() { ["error"] = "google_failed" });
        }

        var principal = result.Principal;
        var key = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal.FindFirstValue(ClaimTypes.Email);
        var name = principal.FindFirstValue(ClaimTypes.Name) ?? email;
        var avatar = principal.FindFirstValue(GoogleAuthExtensions.PictureClaim);

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(email))
        {
            return RedirectToFrontend("/login", new() { ["error"] = "google_no_email" });
        }

        string code;
            code = await _mediator.Send(new ExternalLoginCommand(
                new ExternalLogin(GoogleAuthExtensions.ProviderName, key, email, name!, avatar)));

        return RedirectToFrontend("/auth/callback", new()
        {
            ["code"] = code,
            ["returnUrl"] = ReturnUrl.IsValid(returnUrl) ? returnUrl : "/"
        });
    }

    [HttpPost("exchange")]
    public async Task<ActionResult<TokenResponse>> Exchange([FromBody] ExchangeRequest request)
    {
        return Ok(await _mediator.Send(new ExchangeCodeCommand(request.Code)));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponse>> Refresh()
    {
        return Ok(await _mediator.Send(new RefreshCommand()));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _mediator.Send(new LogoutCommand());
        return NoContent();
    }

    [Authorize]
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll()
    {
        await _mediator.Send(new LogoutAllCommand(User.GetUserId()));
        return NoContent();
    }

    private RedirectResult RedirectToFrontend(string path, Dictionary<string, string?> query)
    {
        var url = QueryHelpers.AddQueryString(_frontend.Url.TrimEnd('/') + path, query);
        return Redirect(url);
    }
}

public sealed record ExchangeRequest(string Code);
