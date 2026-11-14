using FluentValidation;
using MediatR;
using SlipStream.Identity.Core.Contracts;
using ValidationException = SlipStream.Common.ValidationException;

namespace SlipStream.Identity.Core.Sessions;

public sealed record ExchangeCodeCommand(string Code) : IRequest<TokenResponse>;

public class ExchangeCodeValidator : AbstractValidator<ExchangeCodeCommand>
{
    public ExchangeCodeValidator()
    {
        RuleFor(x => x.Code).NotEmpty().OverridePropertyName("code").WithName("Код");
    }
}

public class ExchangeCodeCommandHandler(IAuthCodeStore codes, IAccountService accounts, IJwtProvider jwt)
    : IRequestHandler<ExchangeCodeCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(ExchangeCodeCommand request, CancellationToken ct)
    {
        var userId = await codes.ConsumeAsync(request.Code, ct);
        var account = userId is null ? null : await accounts.FindByIdAsync(userId.Value, ct);

        if (account is null)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["code"] = ["Код недійсний або вже використаний."]
            });

        return await jwt.IssueAsync(account, ct);
    }
}
