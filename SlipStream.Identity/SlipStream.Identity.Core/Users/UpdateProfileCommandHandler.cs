using FluentValidation;
using MediatR;
using SlipStream.Common;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Domain.Users;

namespace SlipStream.Identity.Core.Users;

public readonly record struct Optional<T>(bool HasValue, T? Value)
{
    public static Optional<T> None => default;
    public static Optional<T> Of(T? value) => new(true, value);
}

public sealed record UpdateProfileCommand(
    Guid UserId,
    Optional<string> Handle,
    Optional<string> Name,
    Optional<string> AvatarUrl,
    Optional<string> FavouriteTeam,
    Optional<string> FavouriteDriver) : IRequest<MeResponse>;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.Handle.Value)
            .Must(UserHandle.IsValid).WithMessage(UserHandle.FormatMessage)
            .When(x => x.Handle.HasValue)
            .OverridePropertyName("handle");

        RuleFor(x => x.Name.Value)
            .NotEmpty().MaximumLength(64)
            .When(x => x.Name.HasValue)
            .OverridePropertyName("name").WithName("Ім'я");

        RuleFor(x => x.AvatarUrl.Value)
            .MaximumLength(512)
            .Must(BeHttpUrl).WithMessage("Аватар має бути абсолютним http(s)-посиланням.")
            .When(x => x.AvatarUrl is { HasValue: true, Value: not null })
            .OverridePropertyName("avatarUrl").WithName("Аватар");

        RuleFor(x => x.FavouriteTeam.Value)
            .Must(t => RacingReference.IsKnownTeam(t!)).WithMessage("Невідома команда.")
            .When(x => x.FavouriteTeam is { HasValue: true, Value: not null })
            .OverridePropertyName("favouriteTeam");

        RuleFor(x => x.FavouriteDriver.Value)
            .Must(d => RacingReference.IsKnownDriver(d!)).WithMessage("Невідомий код пілота.")
            .When(x => x.FavouriteDriver is { HasValue: true, Value: not null })
            .OverridePropertyName("favouriteDriver");
    }

    private static bool BeHttpUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}

public class UpdateProfileCommandHandler(IUserRepository users, IAccountService accounts)
    : IRequestHandler<UpdateProfileCommand, MeResponse>
{
    public async Task<MeResponse> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        if (request.Handle.HasValue &&
            await users.IsHandleTakenAsync(request.Handle.Value!, excludeUserId: request.UserId, ct))
        {
            throw new ConflictException("Цей нік уже зайнятий.");
        }

        var found = await users.PatchAsync(request.UserId, user =>
        {
            if (request.Handle.HasValue) user.ChangeHandle(request.Handle.Value!);
            if (request.Name.HasValue) user.Rename(request.Name.Value!);
            if (request.AvatarUrl.HasValue) user.SetAvatar(request.AvatarUrl.Value);
            if (request.FavouriteTeam.HasValue) user.SetFavouriteTeam(Canonical(RacingReference.Teams, request.FavouriteTeam.Value));
            if (request.FavouriteDriver.HasValue) user.SetFavouriteDriver(request.FavouriteDriver.Value?.ToUpperInvariant());
        }, ct);

        if (!found)
            throw new NotFoundException("Користувача не знайдено.");

        var updated = await users.GetByIdAsync(request.UserId, ct);
        var providers = await accounts.GetProvidersAsync(request.UserId, ct);
        return MeResponse.From(updated!, providers);
    }

    private static string? Canonical(IReadOnlySet<string> set, string? value) =>
        value is null ? null : set.First(v => string.Equals(v, value, StringComparison.OrdinalIgnoreCase));
}
