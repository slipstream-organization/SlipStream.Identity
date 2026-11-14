using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SlipStream.Common.Authentication;

namespace SlipStream.Identity.ApiTests.Authentication;

public sealed class JwtBearerValidationTests : IAsyncLifetime
{
    private const string Secret = "test-secret-key-at-least-32-characters-long!";
    private const string Issuer = "https://identity.test";
    private const string Audience = "slipstream-api";

    private IHost _host = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _host = await new HostBuilder()
            .ConfigureWebHost(web => web
                .UseTestServer()
                .ConfigureAppConfiguration(c => c.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SecretKey"] = Secret,
                    ["Jwt:Issuer"] = Issuer,
                    ["Jwt:Audience"] = Audience
                }))
                .ConfigureServices((ctx, services) =>
                {
                    services.AddRouting();
                    services.AddJwtAuthentication(ctx.Configuration);
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(e => e.MapGet("/protected", (ClaimsPrincipal user) =>
                        Results.Ok(user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value)).RequireAuthorization());
                }))
            .StartAsync();

        _client = _host.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _host.StopAsync();
        _host.Dispose();
    }

    [Fact]
    public async Task Valid_token_passes()
    {
        var response = await CallWithBearer(CreateToken());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Valid_token_from_cookie_passes()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/protected");
        request.Headers.Add("Cookie", $"{JwtAuthenticationExtensions.AccessTokenCookie}={CreateToken()}");

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Wrong_audience_is_rejected()
    {
        var response = await CallWithBearer(CreateToken(audience: "other-api"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Wrong_issuer_is_rejected()
    {
        var response = await CallWithBearer(CreateToken(issuer: "https://evil.test"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Wrong_signing_key_is_rejected()
    {
        var response = await CallWithBearer(CreateToken(secret: "another-secret-key-at-least-32-characters!!"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Expired_token_is_rejected()
    {
        var response = await CallWithBearer(CreateToken(expires: DateTime.UtcNow.AddMinutes(-5)));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Missing_token_is_rejected()
    {
        var response = await _client.GetAsync("/protected");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private Task<HttpResponseMessage> CallWithBearer(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/protected");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client.SendAsync(request);
    }

    private static string CreateToken(
        string issuer = Issuer, string audience = Audience, string secret = Secret, DateTime? expires = null)
    {
        var expiresAt = expires ?? DateTime.UtcNow.AddMinutes(15);
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())]),
            Issuer = issuer,
            Audience = audience,
            NotBefore = expiresAt.AddMinutes(-30),
            IssuedAt = expiresAt.AddMinutes(-30),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), SecurityAlgorithms.HmacSha256)
        });
    }
}
