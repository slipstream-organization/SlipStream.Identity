using MediatR;
using Microsoft.AspNetCore.Mvc;
using SlipStream.Identity.Common;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Core.Registration;
using SlipStream.Identity.Core.Sessions;

namespace SlipStream.Identity.Controllers;

[ApiController]
[Route("v1/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var id = await mediator.Send(new RegisterCommand(request.UserName, request.Email, request.Password));
        return StatusCode(StatusCodes.Status201Created, new RegisterResponse(id, request.UserName, request.Email));
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest request)
    {
        return Ok(await mediator.Send(new LoginCommand(request.Email, request.Password)));
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
}
