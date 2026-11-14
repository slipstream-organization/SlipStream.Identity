using Microsoft.AspNetCore.Identity;

namespace SlipStream.Identity.Infrastructure.Identity;

internal sealed class UkrainianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() =>
        Error(nameof(DefaultError), "Сталася невідома помилка.");

    public override IdentityError ConcurrencyFailure() =>
        Error(nameof(ConcurrencyFailure), "Дані було змінено іншим запитом. Спробуйте ще раз.");

    public override IdentityError PasswordMismatch() =>
        Error(nameof(PasswordMismatch), "Неправильний пароль.");

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

    public override IdentityError UserAlreadyHasPassword() =>
        Error(nameof(UserAlreadyHasPassword), "Користувач уже має пароль.");

    public override IdentityError UserLockoutNotEnabled() =>
        Error(nameof(UserLockoutNotEnabled), "Блокування для цього користувача не ввімкнене.");

    public override IdentityError UserAlreadyInRole(string role) =>
        Error(nameof(UserAlreadyInRole), $"Користувач уже має роль '{role}'.");

    public override IdentityError UserNotInRole(string role) =>
        Error(nameof(UserNotInRole), $"Користувач не має ролі '{role}'.");

    public override IdentityError PasswordTooShort(int length) =>
        Error(nameof(PasswordTooShort), $"Пароль має містити щонайменше {length} символів.");

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
        Error(nameof(PasswordRequiresUniqueChars), $"Пароль має містити щонайменше {uniqueChars} різних символів.");

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        Error(nameof(PasswordRequiresNonAlphanumeric), "Пароль має містити хоча б один спецсимвол.");

    public override IdentityError PasswordRequiresDigit() =>
        Error(nameof(PasswordRequiresDigit), "Пароль має містити хоча б одну цифру.");

    public override IdentityError PasswordRequiresLower() =>
        Error(nameof(PasswordRequiresLower), "Пароль має містити хоча б одну малу літеру.");

    public override IdentityError PasswordRequiresUpper() =>
        Error(nameof(PasswordRequiresUpper), "Пароль має містити хоча б одну велику літеру.");

    public override IdentityError RecoveryCodeRedemptionFailed() =>
        Error(nameof(RecoveryCodeRedemptionFailed), "Не вдалося використати код відновлення.");

    private static IdentityError Error(string code, string description) =>
        new() { Code = code, Description = description };
}
