using SlipStream.Common;
using SlipStream.Identity.Core.Sessions;
using SlipStream.Identity.UnitTests.Contracts;

namespace SlipStream.Identity.UnitTests.Sessions;

public sealed class ExchangeCodeCommandHandlerTests
{
    private readonly FakeAuthCodeStore _codes = new();
    private readonly FakeAccountService _accounts = new();
    private readonly FakeJwtProvider _jwt = new();

    private ExchangeCodeCommandHandler Handler => new(_codes, _accounts, _jwt);

    [Fact]
    public async Task Valid_code_issues_tokens_for_its_user()
    {
        var userId = Guid.NewGuid();
        _accounts.Add(userId, "u@test.dev");
        var code = await _codes.CreateAsync(userId);

        var result = await Handler.Handle(new ExchangeCodeCommand(code), default);

        Assert.Equal(_jwt.Token, result);
        Assert.Equal([userId], _jwt.IssuedFor);
    }

    [Fact]
    public async Task Code_can_be_used_only_once()
    {
        var userId = Guid.NewGuid();
        _accounts.Add(userId, "u@test.dev");
        var code = await _codes.CreateAsync(userId);

        await Handler.Handle(new ExchangeCodeCommand(code), default);

        await Assert.ThrowsAsync<ValidationException>(() => Handler.Handle(new ExchangeCodeCommand(code), default));
        Assert.Single(_jwt.IssuedFor);
    }

    [Fact]
    public async Task Unknown_code_is_rejected()
    {
        await Assert.ThrowsAsync<ValidationException>(() => Handler.Handle(new ExchangeCodeCommand("unknown"), default));
        Assert.Empty(_jwt.IssuedFor);
    }

    [Fact]
    public async Task Code_of_deleted_account_is_rejected()
    {
        var code = await _codes.CreateAsync(Guid.NewGuid());

        await Assert.ThrowsAsync<ValidationException>(() => Handler.Handle(new ExchangeCodeCommand(code), default));
        Assert.Empty(_jwt.IssuedFor);
    }

    [Fact]
    public void Empty_code_fails_validation()
    {
        var result = new ExchangeCodeValidator().Validate(new ExchangeCodeCommand(""));
        Assert.Contains(result.Errors, e => e.PropertyName == "code");
    }
}
