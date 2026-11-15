using SlipStream.Identity.Core.Users;

namespace SlipStream.Identity.UnitTests.Users;

public sealed class UpdateProfileValidatorTests
{
    private readonly UpdateProfileValidator _validator;

    public UpdateProfileValidatorTests()
    {
        _validator = new UpdateProfileValidator();
    }

    private static UpdateProfileCommand Command(
        Optional<string> handle = default, Optional<string> name = default, Optional<string> avatar = default,
        Optional<string> team = default, Optional<string> driver = default)
    {
        return new UpdateProfileCommand(Guid.NewGuid(), handle, name, avatar, team, driver);
    }

    [Fact]
    public void Empty_request_is_valid()
    {
        Assert.True(_validator.Validate(Command()).IsValid);
    }

    [Fact]
    public void Known_team_and_driver_pass()
    {
        var result = _validator.Validate(Command(team: Optional<string>.Of("McLaren"), driver: Optional<string>.Of("nor")));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Unknown_team_or_driver_fails()
    {
        var result = _validator.Validate(Command(team: Optional<string>.Of("brawn_gp"), driver: Optional<string>.Of("XXX")));

        Assert.Contains(result.Errors, e => e.PropertyName == "favouriteTeam");
        Assert.Contains(result.Errors, e => e.PropertyName == "favouriteDriver");
    }

    [Fact]
    public void Null_team_and_driver_are_allowed()
    {
        var result = _validator.Validate(Command(team: Optional<string>.Of(null), driver: Optional<string>.Of(null)));
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("x")]
    [InlineData("bad handle")]
    public void Invalid_handle_fails(string? handle)
    {
        var result = _validator.Validate(Command(handle: Optional<string>.Of(handle)));
        Assert.Contains(result.Errors, e => e.PropertyName == "handle");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Empty_name_fails(string? name)
    {
        var result = _validator.Validate(Command(name: Optional<string>.Of(name)));
        Assert.Contains(result.Errors, e => e.PropertyName == "name");
    }

    [Theory]
    [InlineData("ftp://img.test/a.png")]
    [InlineData("not-a-url")]
    public void Non_http_avatar_fails(string avatar)
    {
        var result = _validator.Validate(Command(avatar: Optional<string>.Of(avatar)));
        Assert.Contains(result.Errors, e => e.PropertyName == "avatarUrl");
    }
}
