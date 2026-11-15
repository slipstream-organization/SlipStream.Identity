using SlipStream.Identity.Core.Users;
using SlipStream.Identity.UnitTests.Contracts;

namespace SlipStream.Identity.UnitTests.Users;

public sealed class CheckHandleQueryHandlerTests
{
    private readonly FakeUserRepository _users;
    private readonly CheckHandleQueryHandler _handler;

    public CheckHandleQueryHandlerTests()
    {
        _users = new FakeUserRepository();
        _handler = new CheckHandleQueryHandler(_users);
    }

    [Fact]
    public async Task Free_handle_is_available()
    {
        var result = await _handler.Handle(new CheckHandleQuery("free_handle"), default);
        Assert.True(result.Available);
    }

    [Fact]
    public async Task Taken_handle_is_not_available_ignoring_case()
    {
        _users.Add("Max_33");

        var result = await _handler.Handle(new CheckHandleQuery("max_33"), default);

        Assert.False(result.Available);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("has space")]
    [InlineData("кирилиця")]
    [InlineData("dash-name")]
    [InlineData("a_very_long_handle_that_is_over_32")]
    public async Task Invalid_handle_is_not_available(string? handle)
    {
        var result = await _handler.Handle(new CheckHandleQuery(handle), default);
        Assert.False(result.Available);
    }
}
