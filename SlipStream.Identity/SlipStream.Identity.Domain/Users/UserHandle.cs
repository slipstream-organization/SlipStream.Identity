using System.Text.RegularExpressions;

namespace SlipStream.Identity.Domain.Users;

public static partial class UserHandle
{
    public const int MinLength = 3;
    public const int MaxLength = 32;
    public const string FormatMessage = "Нік має містити 3–32 символи: латинські літери, цифри або _.";

    public static bool IsValid(string? handle)
    {
        return handle is not null && Pattern().IsMatch(handle);
    }

    public static string Normalize(string handle)
    {
        return handle.ToUpperInvariant();
    }

    [GeneratedRegex("^[A-Za-z0-9_]{3,32}$")]
    private static partial Regex Pattern();
}
