using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using SlipStream.Identity.Core.Contracts;
using StackExchange.Redis;

namespace SlipStream.Identity.Infrastructure.Identity;

internal sealed class RedisAuthCodeStore : IAuthCodeStore
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(60);

    private readonly IConnectionMultiplexer _redis;

    public RedisAuthCodeStore(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<string> CreateAsync(Guid userId, CancellationToken ct = default)
    {
        var code = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        await _redis.GetDatabase().StringSetAsync(Key(code), userId.ToString(), Lifetime);
        return code;
    }

    public async Task<Guid?> ConsumeAsync(string code, CancellationToken ct = default)
    {
        var value = await _redis.GetDatabase().StringGetDeleteAsync(Key(code));
        if (!value.HasValue || !Guid.TryParse(value.ToString(), out var userId))
        {
            return null;
        }

        return userId;
    }

    private static string Key(string code)
    {
        return $"authcode:{code}";
    }
}
