using SlipStream.Identity.Core.Sessions;
using SlipStream.Identity.UnitTests.Contracts;

namespace SlipStream.Identity.UnitTests.Sessions;

public sealed class RefreshCommandHandlerTests
{
    [Fact]
    public async Task Returns_new_tokens_from_provider()
    {
        var jwt = new FakeJwtProvider();

        var result = await new RefreshCommandHandler(jwt).Handle(new RefreshCommand(), default);

        Assert.Equal(jwt.Token, result);
        Assert.Equal(1, jwt.RefreshCalls);
    }
}
