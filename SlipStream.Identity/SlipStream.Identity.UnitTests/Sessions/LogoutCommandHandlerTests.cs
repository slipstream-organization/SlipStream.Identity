using SlipStream.Identity.Core.Sessions;
using SlipStream.Identity.UnitTests.Contracts;

namespace SlipStream.Identity.UnitTests.Sessions;

public sealed class LogoutCommandHandlerTests
{
    [Fact]
    public async Task Revokes_current_session()
    {
        var jwt = new FakeJwtProvider();

        await new LogoutCommandHandler(jwt).Handle(new LogoutCommand(), default);

        Assert.Equal(1, jwt.RevokeCalls);
        Assert.Empty(jwt.RevokedAllFor);
    }
}
