using SlipStream.Common;
using SlipStream.Identity.Core.Users;
using SlipStream.Identity.Domain.Users;
using SlipStream.Identity.UnitTests.Contracts;

namespace SlipStream.Identity.UnitTests.Users;

public sealed class UpdateProfileCommandHandlerTests
{
    private readonly FakeUserRepository _users;
    private readonly User _me;
    private readonly UpdateProfileCommandHandler _handler;

    public UpdateProfileCommandHandlerTests()
    {
        _users = new FakeUserRepository();
        _me = User.Create(Guid.NewGuid(), "me@test.dev", "my_handle", "Me", "https://img.test/me.png");
        _me.SetFavouriteTeam("ferrari");
        _me.SetFavouriteDriver("LEC");
        _users.Users.Add(_me);
        _users.Add("taken_handle");
        _handler = new UpdateProfileCommandHandler(_users, new FakeAccountService());
    }

    private UpdateProfileCommand Command(
        Optional<string> handle = default, Optional<string> name = default, Optional<string> avatar = default,
        Optional<string> team = default, Optional<string> driver = default)
    {
        return new UpdateProfileCommand(_me.Id, handle, name, avatar, team, driver);
    }

    [Fact]
    public async Task Field_not_in_request_is_not_changed()
    {
        await _handler.Handle(Command(name: Optional<string>.Of("New name")), default);

        Assert.Equal("New name", _me.DisplayName);
        Assert.Equal("my_handle", _me.Handle);
        Assert.Equal("https://img.test/me.png", _me.AvatarUrl);
        Assert.Equal("ferrari", _me.FavouriteTeam);
        Assert.Equal("LEC", _me.FavouriteDriver);
    }

    [Fact]
    public async Task Null_clears_field()
    {
        await _handler.Handle(Command(
            avatar: Optional<string>.Of(null),
            team: Optional<string>.Of(null),
            driver: Optional<string>.Of(null)), default);

        Assert.Null(_me.AvatarUrl);
        Assert.Null(_me.FavouriteTeam);
        Assert.Null(_me.FavouriteDriver);
    }

    [Fact]
    public async Task Team_and_driver_are_stored_in_canonical_form()
    {
        await _handler.Handle(Command(team: Optional<string>.Of("McLaren"), driver: Optional<string>.Of("nor")), default);

        Assert.Equal("mclaren", _me.FavouriteTeam);
        Assert.Equal("NOR", _me.FavouriteDriver);
    }

    [Fact]
    public async Task Returns_updated_profile()
    {
        var result = await _handler.Handle(Command(handle: Optional<string>.Of("new_handle")), default);

        Assert.Equal("new_handle", result.Handle);
        Assert.Equal(_me.Id, result.Id);
    }

    [Fact]
    public async Task Taken_handle_gives_conflict()
    {
        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(Command(handle: Optional<string>.Of("TAKEN_handle")), default));

        Assert.Equal("my_handle", _me.Handle);
    }

    [Fact]
    public async Task Own_handle_is_not_considered_taken()
    {
        var result = await _handler.Handle(Command(handle: Optional<string>.Of("My_Handle")), default);
        Assert.Equal("My_Handle", result.Handle);
    }

    [Fact]
    public async Task Missing_user_gives_not_found()
    {
        var command = Command(name: Optional<string>.Of("X")) with { UserId = Guid.NewGuid() };
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, default));
    }
}
