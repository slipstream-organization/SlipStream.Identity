namespace SlipStream.Identity.Common;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed record RegisterResponse(Guid Id, string UserName, string Email);
