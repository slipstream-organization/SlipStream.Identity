using SlipStream.Common;
using SlipStream.Identity.Core.Users;
using SlipStream.Identity.UnitTests.Contracts;

namespace SlipStream.Identity.UnitTests.Users;

public sealed class GetMeQueryHandlerTests
{
    private readonly FakeUserRepository _users;
    private readonly FakeAccountService _accounts;
    private readonly GetMeQueryHandler _handler;

    public GetMeQueryHandlerTests()
    {
        _users = new FakeUserRepository();
        _accounts = new FakeAccountService();
        _handler = new GetMeQueryHandler(_users, _accounts);
    }

    [Fact]
    public async Task Returns_all_profile_fields_and_providers()
    {
        var user = _users.Add("lando", "lando@test.dev", "Lando Norris");
        user.SetAvatar("https://img.test/lando.png");
        user.SetFavouriteTeam("mclaren");
        user.SetFavouriteDriver("NOR");
        _accounts.Add(user.Id, user.Email, googleKey: "g-4");

        var me = await _handler.Handle(new GetMeQuery(user.Id), default);

        Assert.Equal(user.Id, me.Id);
        Assert.Equal("lando", me.Handle);
        Assert.Equal("Lando Norris", me.Name);
        Assert.Equal("lando@test.dev", me.Email);
        Assert.Equal("https://img.test/lando.png", me.AvatarUrl);
        Assert.Equal("mclaren", me.FavouriteTeam);
        Assert.Equal("NOR", me.FavouriteDriver);
        Assert.Equal(user.CreatedAtUtc, me.CreatedAt);
        Assert.Equal(["google"], me.Providers);
    }

    [Fact]
    public async Task Missing_profile_gives_not_found()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new GetMeQuery(Guid.NewGuid()), default));
    }
}
