using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SlipStream.Common;
using SlipStream.Common.Authentication;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Domain.Tokens;

namespace SlipStream.Identity.Infrastructure.Providers;

internal sealed class JwtProvider(
    IOptions<JwtProviderOptions> options,
    IRefreshTokenRepository refreshTokens,
    IAccountService accounts,
    IHttpContextAccessor httpContextAccessor) : IJwtProvider
{
    public const string RefreshTokenCookie = "refresh_token";
    private const string RefreshCookiePath = "/v1/auth";

    private static readonly JsonWebTokenHandler Handler = new();
    private readonly JwtProviderOptions _options = options.Value;

    private HttpContext Http => httpContextAccessor.HttpContext
                                ?? throw new InvalidOperationException("JwtProvider використовується поза HTTP-запитом.");

    public async Task<TokenResponse> IssueAsync(AccountInfo account, CancellationToken ct = default)
    {
        var (accessToken, accessExpires) = GenerateAccessToken(account);

        var rawRefresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshExpires = DateTime.UtcNow.AddDays(_options.RefreshTokenDays);
        await refreshTokens.AddAsync(RefreshToken.Create(account.Id, Hash(rawRefresh), refreshExpires), ct);

        SetCookie(JwtAuthenticationExtensions.AccessTokenCookie, accessToken, accessExpires, "/");
        SetCookie(RefreshTokenCookie, rawRefresh, refreshExpires, RefreshCookiePath);

        return new TokenResponse(accessToken, accessExpires);
    }

    public async Task<TokenResponse> RefreshAsync(CancellationToken ct = default)
    {
        var raw = Http.Request.Cookies[RefreshTokenCookie];
        if (string.IsNullOrEmpty(raw))
            throw new UnauthorizedException("Недійсний refresh-токен. Увійдіть знову.");

        var stored = await refreshTokens.GetByHashAsync(Hash(raw), ct);
        if (stored is null)
            throw new UnauthorizedException("Недійсний refresh-токен. Увійдіть знову.");

        // Refresh одноразовий: старий видаляємо в будь-якому разі.
        await refreshTokens.DeleteAsync(stored, ct);

        if (stored.IsExpired)
            throw new UnauthorizedException("Термін дії refresh-токена минув. Увійдіть знову.");

        var account = await accounts.FindByIdAsync(stored.UserId, ct)
                      ?? throw new UnauthorizedException("Недійсний refresh-токен. Увійдіть знову.");

        return await IssueAsync(account, ct);
    }

    public async Task RevokeAsync(CancellationToken ct = default)
    {
        var raw = Http.Request.Cookies[RefreshTokenCookie];
        if (!string.IsNullOrEmpty(raw))
        {
            var stored = await refreshTokens.GetByHashAsync(Hash(raw), ct);
            if (stored is not null)
                await refreshTokens.DeleteAsync(stored, ct);
        }

        Http.Response.Cookies.Delete(JwtAuthenticationExtensions.AccessTokenCookie, new CookieOptions { Path = "/" });
        Http.Response.Cookies.Delete(RefreshTokenCookie, new CookieOptions { Path = RefreshCookiePath });
    }

    private (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(AccountInfo account)
    {
        var expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, account.Email),
                new Claim(JwtRegisteredClaimNames.Name, account.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ]),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Expires = expires,
            SigningCredentials = new SigningCredentials(_options.CreateSigningKey(), SecurityAlgorithms.HmacSha256)
        };

        return (Handler.CreateToken(descriptor), expires);
    }

    private void SetCookie(string name, string value, DateTime expiresAtUtc, string path) =>
        Http.Response.Cookies.Append(name, value, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = path,
            Expires = expiresAtUtc
        });

    private static string Hash(string raw) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
}

