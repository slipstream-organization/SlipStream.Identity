using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using SlipStream.Identity.Core.Contracts;
using StackExchange.Redis;

namespace SlipStream.Identity.Infrastructure.Identity;

internal sealed class RedisAuthCodeStore(IConnectionMultiplexer redis) : IAuthCodeStore
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(60);

    public async Task<string> CreateAsync(Guid userId, CancellationToken ct = default)
    {
        var code = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        await redis.GetDatabase().StringSetAsync(Key(code), userId.ToString(), Lifetime);
        return code;
    }

    public async Task<Guid?> ConsumeAsync(string code, CancellationToken ct = default)
    {
        var value = await redis.GetDatabase().StringGetDeleteAsync(Key(code));
        return value.HasValue && Guid.TryParse(value.ToString(), out var userId) ? userId : null;
    }

    private static string Key(string code) => $"authcode:{code}";
}
