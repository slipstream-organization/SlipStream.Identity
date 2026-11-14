using SlipStream.Identity.Core.Sessions;
using SlipStream.Identity.UnitTests.Contracts;

namespace SlipStream.Identity.UnitTests.Sessions;

public sealed class LogoutAllCommandHandlerTests
{
    [Fact]
    public async Task Revokes_all_sessions_of_the_user()
    {
        var jwt = new FakeJwtProvider();
        var userId = Guid.NewGuid();

        await new LogoutAllCommandHandler(jwt).Handle(new LogoutAllCommand(userId), default);

        Assert.Equal([userId], jwt.RevokedAllFor);
        Assert.Equal(0, jwt.RevokeCalls);
    }
}
