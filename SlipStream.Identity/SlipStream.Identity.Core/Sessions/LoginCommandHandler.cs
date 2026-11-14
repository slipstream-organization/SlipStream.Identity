using FluentValidation;
using MediatR;
using SlipStream.Common;
using SlipStream.Identity.Core.Contracts;

namespace SlipStream.Identity.Core.Sessions;

public sealed record LoginCommand(string email, string password) : IRequest<TokenResponse>;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.email).NotEmpty().WithName("Email");
        RuleFor(x => x.password).NotEmpty().WithName("Пароль");
    }
}

public class LoginCommandHandler(IAccountService accounts, IUserRepository users, IJwtProvider jwt)
    : IRequestHandler<LoginCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var account = await accounts.ValidateCredentialsAsync(request.email, request.password, ct)
                      ?? throw new UnauthorizedException("Неправильні дані для входу");

        await users.PatchAsync(account.Id, u => u.MarkLoggedIn(), ct);

        return await jwt.IssueAsync(account, ct);
    }
}
