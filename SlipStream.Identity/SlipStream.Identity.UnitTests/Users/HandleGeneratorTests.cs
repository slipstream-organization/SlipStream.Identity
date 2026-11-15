using SlipStream.Identity.Core.Users;
using SlipStream.Identity.Domain.Users;
using SlipStream.Identity.UnitTests.Contracts;

namespace SlipStream.Identity.UnitTests.Users;

public sealed class HandleGeneratorTests
{
    [Theory]
    [InlineData("Lewis Hamilton", "lewis_hamilton")]
    [InlineData("  kimi.antonelli  ", "kimi_antonelli")]
    [InlineData("Олег", "racer")]
    [InlineData("A", "racer")]
    [InlineData("Very Long Name That Exceeds Twenty", "very_long_name_that")]
    public void Base_handle_is_derived_from_name(string seed, string expected)
    {
        Assert.Equal(expected, HandleGenerator.ToBase(seed));
    }

    [Fact]
    public async Task Free_base_handle_is_used_as_is()
    {
        var handle = await HandleGenerator.GenerateAsync(new FakeUserRepository(), "Lando Norris");
        Assert.Equal("lando_norris", handle);
    }

    [Fact]
    public async Task Taken_base_handle_gets_suffix_and_stays_valid()
    {
        var users = new FakeUserRepository();
        users.Add("lewis_hamilton");

        var handle = await HandleGenerator.GenerateAsync(users, "Lewis Hamilton");

        Assert.StartsWith("lewis_hamilton_", handle);
        Assert.True(UserHandle.IsValid(handle));
        Assert.False(await users.IsHandleTakenAsync(handle));
    }
}
