using Microsoft.AspNetCore.Identity;

namespace SlipStream.Identity.Infrastructure.Identity;

internal sealed class UkrainianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError()
    {
        return Error(nameof(DefaultError), "Сталася невідома помилка.");
    }

    public override IdentityError ConcurrencyFailure()
    {
        return Error(nameof(ConcurrencyFailure), "Дані було змінено іншим запитом. Спробуйте ще раз.");
    }

    public override IdentityError InvalidToken()
    {
        return Error(nameof(InvalidToken), "Недійсний токен.");
    }

    public override IdentityError LoginAlreadyAssociated()
    {
        return Error(nameof(LoginAlreadyAssociated), "Користувач із таким логіном уже існує.");
    }

    public override IdentityError InvalidUserName(string? userName)
    {
        return Error(nameof(InvalidUserName),
            $"Ім'я користувача '{userName}' недійсне. Дозволені лише латинські літери, цифри та символи _ . -");
    }

    public override IdentityError InvalidEmail(string? email)
    {
        return Error(nameof(InvalidEmail), $"Email '{email}' недійсний.");
    }

    public override IdentityError DuplicateUserName(string userName)
    {
        return Error(nameof(DuplicateUserName), $"Ім'я користувача '{userName}' уже зайняте.");
    }

    public override IdentityError DuplicateEmail(string email)
    {
        return Error(nameof(DuplicateEmail), $"Email '{email}' уже зареєстрований.");
    }

    public override IdentityError InvalidRoleName(string? role)
    {
        return Error(nameof(InvalidRoleName), $"Назва ролі '{role}' недійсна.");
    }

    public override IdentityError DuplicateRoleName(string role)
    {
        return Error(nameof(DuplicateRoleName), $"Роль '{role}' уже існує.");
    }

    public override IdentityError UserAlreadyInRole(string role)
    {
        return Error(nameof(UserAlreadyInRole), $"Користувач уже має роль '{role}'.");
    }

    public override IdentityError UserNotInRole(string role)
    {
        return Error(nameof(UserNotInRole), $"Користувач не має ролі '{role}'.");
    }

    private static IdentityError Error(string code, string description)
    {
        return new IdentityError { Code = code, Description = description };
    }
}
