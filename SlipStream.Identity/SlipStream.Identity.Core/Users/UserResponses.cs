using SlipStream.Identity.Domain.Users;

namespace SlipStream.Identity.Core.Users;

public sealed record MeResponse(
    Guid Id,
    string Handle,
    string Name,
    string Email,
    string? AvatarUrl,
    string? FavouriteTeam,
    string? FavouriteDriver,
    DateTime CreatedAt,
    IReadOnlyList<string> Providers)
{
    public static MeResponse From(User user, IReadOnlyList<string> providers)
    {
        return new MeResponse(
            user.Id, user.Handle, user.DisplayName, user.Email, user.AvatarUrl,
            user.FavouriteTeam, user.FavouriteDriver, user.CreatedAtUtc, providers);
    }
}

public sealed record PublicUserResponse(
    string Handle,
    string Name,
    string? AvatarUrl,
    string? FavouriteTeam,
    string? FavouriteDriver,
    DateTime CreatedAt)
{
    public static PublicUserResponse From(User user)
    {
        return new PublicUserResponse(
            user.Handle, user.DisplayName, user.AvatarUrl, user.FavouriteTeam, user.FavouriteDriver, user.CreatedAtUtc);
    }
}

public sealed record HandleAvailabilityResponse(bool Available);
