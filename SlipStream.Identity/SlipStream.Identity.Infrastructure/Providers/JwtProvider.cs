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

internal sealed class JwtProvider : IJwtProvider
{
    public const string RefreshTokenCookie = "refresh_token";
    private const string RefreshCookiePath = "/v1/auth";

    private static readonly JsonWebTokenHandler Handler = new();

    private readonly JwtProviderOptions _options;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IAccountService _accounts;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtProvider(
        IOptions<JwtProviderOptions> options,
        IRefreshTokenRepository refreshTokens,
        IAccountService accounts,
        IHttpContextAccessor httpContextAccessor)
    {
        _options = options.Value;
        _refreshTokens = refreshTokens;
        _accounts = accounts;
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext Http => _httpContextAccessor.HttpContext
                                ?? throw new InvalidOperationException("JwtProvider використовується поза HTTP-запитом.");

    public async Task<TokenResponse> IssueAsync(AccountInfo account, CancellationToken ct = default)
    {
        var (accessToken, accessExpires) = GenerateAccessToken(account);

        var rawRefresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshExpires = DateTime.UtcNow.AddDays(_options.RefreshTokenDays);
        await _refreshTokens.AddAsync(RefreshToken.Create(account.Id, Hash(rawRefresh), refreshExpires), ct);

        SetCookie(JwtAuthenticationExtensions.AccessTokenCookie, accessToken, accessExpires, "/");
        SetCookie(RefreshTokenCookie, rawRefresh, refreshExpires, RefreshCookiePath);

        return new TokenResponse(accessToken, accessExpires);
    }

    public async Task<TokenResponse> RefreshAsync(CancellationToken ct = default)
    {
        var raw = Http.Request.Cookies[RefreshTokenCookie];
        if (string.IsNullOrEmpty(raw))
        {
            throw new UnauthorizedException("Недійсний refresh-токен. Увійдіть знову.");
        }

        var stored = await _refreshTokens.GetByHashAsync(Hash(raw), ct);
        if (stored is null)
        {
            throw new UnauthorizedException("Недійсний refresh-токен. Увійдіть знову.");
        }

        await _refreshTokens.DeleteAsync(stored, ct);

        if (stored.IsExpired)
        {
            throw new UnauthorizedException("Термін дії refresh-токена минув. Увійдіть знову.");
        }

        var account = await _accounts.FindByIdAsync(stored.UserId, ct);
        if (account is null)
        {
            throw new UnauthorizedException("Недійсний refresh-токен. Увійдіть знову.");
        }

        return await IssueAsync(account, ct);
    }

    public async Task RevokeAsync(CancellationToken ct = default)
    {
        var raw = Http.Request.Cookies[RefreshTokenCookie];
        if (!string.IsNullOrEmpty(raw))
        {
            var stored = await _refreshTokens.GetByHashAsync(Hash(raw), ct);
            if (stored is not null)
            {
                await _refreshTokens.DeleteAsync(stored, ct);
            }
        }

        ClearCookies();
    }

    public async Task RevokeAllAsync(Guid userId, CancellationToken ct = default)
    {
        await _refreshTokens.DeleteAllForUserAsync(userId, ct);
        ClearCookies();
    }

    private void ClearCookies()
    {
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

    private void SetCookie(string name, string value, DateTime expiresAtUtc, string path)
    {
        Http.Response.Cookies.Append(name, value, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = path,
            Expires = expiresAtUtc
        });
    }

    private static string Hash(string raw)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
    }
}
