using Microsoft.AspNetCore.Identity;

namespace SlipStream.Identity.Infrastructure.Identity;

internal sealed class UkrainianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() =>
        Error(nameof(DefaultError), "Сталася невідома помилка.");

    public override IdentityError ConcurrencyFailure() =>
        Error(nameof(ConcurrencyFailure), "Дані було змінено іншим запитом. Спробуйте ще раз.");

    public override IdentityError InvalidToken() =>
        Error(nameof(InvalidToken), "Недійсний токен.");

    public override IdentityError LoginAlreadyAssociated() =>
        Error(nameof(LoginAlreadyAssociated), "Користувач із таким логіном уже існує.");

    public override IdentityError InvalidUserName(string? userName) =>
        Error(nameof(InvalidUserName),
            $"Ім'я користувача '{userName}' недійсне. Дозволені лише латинські літери, цифри та символи _ . -");

    public override IdentityError InvalidEmail(string? email) =>
        Error(nameof(InvalidEmail), $"Email '{email}' недійсний.");

    public override IdentityError DuplicateUserName(string userName) =>
        Error(nameof(DuplicateUserName), $"Ім'я користувача '{userName}' уже зайняте.");

    public override IdentityError DuplicateEmail(string email) =>
        Error(nameof(DuplicateEmail), $"Email '{email}' уже зареєстрований.");

    public override IdentityError InvalidRoleName(string? role) =>
        Error(nameof(InvalidRoleName), $"Назва ролі '{role}' недійсна.");

    public override IdentityError DuplicateRoleName(string role) =>
        Error(nameof(DuplicateRoleName), $"Роль '{role}' уже існує.");

    public override IdentityError UserAlreadyInRole(string role) =>
        Error(nameof(UserAlreadyInRole), $"Користувач уже має роль '{role}'.");

    public override IdentityError UserNotInRole(string role) =>
        Error(nameof(UserNotInRole), $"Користувач не має ролі '{role}'.");

    private static IdentityError Error(string code, string description) =>
        new() { Code = code, Description = description };
}
