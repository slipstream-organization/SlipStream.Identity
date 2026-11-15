using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlipStream.Common;
using SlipStream.Identity.Common;
using SlipStream.Identity.Core.Users;

namespace SlipStream.Identity.Controllers;

[ApiController]
[Route("v1/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<MeResponse>> Me()
    {
        return Ok(await _mediator.Send(new GetMeQuery(User.GetUserId())));
    }

    [Authorize]
    [HttpPatch("me")]
    public async Task<ActionResult<MeResponse>> UpdateMe([FromBody] JsonElement body)
    {
        if (body.ValueKind != JsonValueKind.Object)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["body"] = ["Тіло запиту має бути JSON-об'єктом."]
            });
        }

        var command = new UpdateProfileCommand(
            User.GetUserId(),
            ReadOptional(body, "handle"),
            ReadOptional(body, "name"),
            ReadOptional(body, "avatarUrl"),
            ReadOptional(body, "favouriteTeam"),
            ReadOptional(body, "favouriteDriver"));

        return Ok(await _mediator.Send(command));
    }

    [AllowAnonymous]
    [HttpGet("handle-available")]
    public async Task<ActionResult<HandleAvailabilityResponse>> HandleAvailable([FromQuery] string? handle)
    {
        return Ok(await _mediator.Send(new CheckHandleQuery(handle)));
    }

    [AllowAnonymous]
    [HttpGet("{handle}")]
    public async Task<ActionResult<PublicUserResponse>> GetByHandle(string handle)
    {
        return Ok(await _mediator.Send(new GetPublicProfileQuery(handle)));
    }

    private static Optional<string> ReadOptional(JsonElement body, string name)
    {
        var property = body.EnumerateObject()
            .FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

        return property.Value.ValueKind switch
        {
            JsonValueKind.Undefined => Optional<string>.None,
            JsonValueKind.Null => Optional<string>.Of(null),
            JsonValueKind.String => Optional<string>.Of(property.Value.GetString()),
            _ => throw new ValidationException(new Dictionary<string, string[]>
            {
                [name] = ["Поле має бути рядком або null."]
            })
        };
    }
}
