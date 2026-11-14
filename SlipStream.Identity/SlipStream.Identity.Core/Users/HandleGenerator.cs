using System.Security.Cryptography;
using System.Text;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Domain.Users;

namespace SlipStream.Identity.Core.Users;

public static class HandleGenerator
{
    private const string Fallback = "racer";
    private const int MaxBaseLength = 20;

    public static async Task<string> GenerateAsync(IUserRepository users, string seed, CancellationToken ct = default)
    {
        var baseHandle = ToBase(seed);

        if (!await users.IsHandleTakenAsync(baseHandle, ct: ct))
            return baseHandle;

        for (var i = 0; i < 10; i++)
        {
            var candidate = $"{baseHandle}_{RandomNumberGenerator.GetInt32(1000, 10000)}";
            if (!await users.IsHandleTakenAsync(candidate, ct: ct))
                return candidate;
        }

        return $"{baseHandle}_{Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant()}";
    }

    public static string ToBase(string seed)
    {
        var sb = new StringBuilder();
        foreach (var c in seed.Trim().ToLowerInvariant())
        {
            if (c is >= 'a' and <= 'z' or >= '0' and <= '9')
                sb.Append(c);
            else if (sb.Length > 0 && sb[^1] != '_')
                sb.Append('_');
        }

        var result = sb.ToString().Trim('_');
        if (result.Length > MaxBaseLength)
            result = result[..MaxBaseLength].TrimEnd('_');

        return result.Length >= UserHandle.MinLength ? result : Fallback;
    }
}
