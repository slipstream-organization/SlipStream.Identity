using System.Text.RegularExpressions;
using FluentValidation;

namespace SlipStream.Identity.Core.Registration;

public class RegisterValidator: AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(r => r.userName)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(32)
            .WithName("Ім'я користувача");

        RuleFor(r => r.email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256)
            .WithName("Email");

        RuleFor(r => r.password)
            .MinimumLength(8)
            .Must(IsPasswordCorrect).WithMessage("Пароль має містити латинські літери та хоча б одну цифру.")
            .WithName("Пароль");
    }

    private bool IsPasswordCorrect(string password)
    {
        return Regex.IsMatch(password, "^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{8,}$");
    }
}