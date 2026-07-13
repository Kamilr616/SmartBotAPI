using System.Security.Claims;

namespace SmartBotBlazorApp.Tests;

public class SignalHubAccessTests
{
    private static readonly string ValidKey = new('x', 32);

    [Fact]
    public void AuthenticatedUserDoesNotNeedRobotKey()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "operator")], "test");

        Assert.True(SignalHubAccess.IsAllowed(new ClaimsPrincipal(identity), null, null));
    }

    [Fact]
    public void MatchingRobotKeyAllowsAnonymousConnection()
    {
        Assert.True(SignalHubAccess.IsAllowed(new ClaimsPrincipal(), ValidKey, ValidKey));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("wrong-key")]
    public void MissingOrWrongRobotKeyIsDenied(string? suppliedKey)
    {
        Assert.False(SignalHubAccess.IsAllowed(new ClaimsPrincipal(), ValidKey, suppliedKey));
    }

    [Fact]
    public void ShortConfiguredKeyIsRejected()
    {
        Assert.False(SignalHubAccess.IsAllowed(new ClaimsPrincipal(), "short", "short"));
    }
}
