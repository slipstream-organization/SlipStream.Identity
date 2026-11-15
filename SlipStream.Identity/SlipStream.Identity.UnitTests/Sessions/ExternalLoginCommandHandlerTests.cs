using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Core.Sessions;
using SlipStream.Identity.Domain.Users;
using SlipStream.Identity.UnitTests.Contracts;

namespace SlipStream.Identity.UnitTests.Sessions;

public sealed class ExternalLoginCommandHandlerTests
{
    private readonly FakeAccountService _accounts;
    private readonly FakeUserRepository _users;
    private readonly FakeAuthCodeStore _codes;
    private readonly ExternalLoginCommandHandler _handler;

    public ExternalLoginCommandHandlerTests()
    {
        _accounts = new FakeAccountService();
        _users = new FakeUserRepository();
        _codes = new FakeAuthCodeStore();
        _handler = new ExternalLoginCommandHandler(_accounts, _users, _codes);
    }

    private static ExternalLoginCommand Command(string key = "g-1", string email = "lewis@test.dev")
    {
        return new ExternalLoginCommand(
            new ExternalLogin("Google", key, email, "Lewis Hamilton", "https://img.test/lewis.png"));
    }

    [Fact]
    public async Task New_user_gets_profile_with_name_avatar_and_generated_handle()
    {
        await _handler.Handle(Command(), default);

        var user = Assert.Single(_users.Users);
        Assert.Equal("lewis@test.dev", user.Email);
        Assert.Equal("Lewis Hamilton", user.DisplayName);
        Assert.Equal("https://img.test/lewis.png", user.AvatarUrl);
        Assert.Equal("lewis_hamilton", user.Handle);
        Assert.NotNull(user.LastLoginAtUtc);
    }

    [Fact]
    public async Task Returns_one_time_code_for_the_user()
    {
        var code = await _handler.Handle(Command(), default);

        var user = Assert.Single(_users.Users);
        Assert.Equal(user.Id, _codes.Codes[code]);
    }

    [Fact]
    public async Task Existing_google_login_does_not_create_new_profile()
    {
        var id = Guid.NewGuid();
        _accounts.Add(id, "lewis@test.dev", googleKey: "g-1");
        var profile = User.Create(id, "lewis@test.dev", "lewis44", "Lewis");
        _users.Users.Add(profile);

        await _handler.Handle(Command(), default);

        Assert.Single(_users.Users);
        Assert.Equal("lewis44", profile.Handle);
        Assert.NotNull(profile.LastLoginAtUtc);
    }

    [Fact]
    public async Task Account_with_same_email_is_linked_instead_of_duplicated()
    {
        var id = Guid.NewGuid();
        _accounts.Add(id, "lewis@test.dev");
        _users.Users.Add(User.Create(id, "lewis@test.dev", "lewis44", "Lewis"));

        await _handler.Handle(Command(key: "g-new"), default);

        Assert.Single(_users.Users);
        Assert.Equal(["google"], await _accounts.GetProvidersAsync(id));
    }

    [Fact]
    public async Task Existing_profile_without_avatar_gets_google_avatar()
    {
        var id = Guid.NewGuid();
        _accounts.Add(id, "lewis@test.dev");
        var profile = User.Create(id, "lewis@test.dev", "lewis44", "Lewis");
        _users.Users.Add(profile);

        await _handler.Handle(Command(), default);

        Assert.Equal("https://img.test/lewis.png", profile.AvatarUrl);
    }

    [Fact]
    public async Task Existing_avatar_is_not_overwritten()
    {
        var id = Guid.NewGuid();
        _accounts.Add(id, "lewis@test.dev", googleKey: "g-1");
        var profile = User.Create(id, "lewis@test.dev", "lewis44", "Lewis", "https://img.test/custom.png");
        _users.Users.Add(profile);

        await _handler.Handle(Command(), default);

        Assert.Equal("https://img.test/custom.png", profile.AvatarUrl);
    }

    [Fact]
    public async Task Generated_handle_does_not_clash_with_taken_one()
    {
        _users.Add("lewis_hamilton");

        await _handler.Handle(Command(), default);

        var created = _users.Users.Last();
        Assert.NotEqual("lewis_hamilton", created.Handle);
        Assert.StartsWith("lewis_hamilton_", created.Handle);
    }
}
