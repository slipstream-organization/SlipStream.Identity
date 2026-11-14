using SlipStream.Identity.Common;

namespace SlipStream.Identity.ApiTests.Common;

public sealed class ReturnUrlTests
{
    [Theory]
    [InlineData("/challenge", true)]
    [InlineData("/dashboard?tab=laps", true)]
    [InlineData("https://evil.test", false)]
    [InlineData("//evil.test", false)]
    [InlineData("/\\evil.test", false)]
    [InlineData("/redirect?to=https://evil.test", false)]
    [InlineData("dashboard", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Only_relative_paths_are_allowed(string? url, bool expected) =>
        Assert.Equal(expected, ReturnUrl.IsValid(url));
}
