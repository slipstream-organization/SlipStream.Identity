using SlipStream.Common;
using SlipStream.Identity.Core.Users;
using SlipStream.Identity.UnitTests.Contracts;

namespace SlipStream.Identity.UnitTests.Users;

public sealed class GetPublicProfileQueryHandlerTests
{
    private readonly FakeUserRepository _users;
    private readonly GetPublicProfileQueryHandler _handler;

    public GetPublicProfileQueryHandlerTests()
    {
        _users = new FakeUserRepository();
        _handler = new GetPublicProfileQueryHandler(_users);
    }

    [Fact]
    public async Task Returns_public_fields_found_by_handle_ignoring_case()
    {
        var user = _users.Add("Oscar_P", name: "Oscar Piastri");
        user.SetFavouriteTeam("mclaren");

        var profile = await _handler.Handle(new GetPublicProfileQuery("oscar_p"), default);

        Assert.Equal("Oscar_P", profile.Handle);
        Assert.Equal("Oscar Piastri", profile.Name);
        Assert.Equal("mclaren", profile.FavouriteTeam);
        Assert.Equal(user.CreatedAtUtc, profile.CreatedAt);
    }

    [Fact]
    public void Public_profile_has_no_private_fields()
    {
        var properties = typeof(PublicUserResponse).GetProperties().Select(p => p.Name).ToList();

        Assert.DoesNotContain("Id", properties);
        Assert.DoesNotContain("Email", properties);
        Assert.DoesNotContain("Providers", properties);
    }

    [Theory]
    [InlineData("nobody")]
    [InlineData("x")]
    [InlineData("bad handle")]
    public async Task Unknown_or_invalid_handle_gives_not_found(string handle)
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new GetPublicProfileQuery(handle), default));
    }
}
