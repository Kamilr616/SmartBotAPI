using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;

namespace SmartBotBlazorApp.Tests;

public class SignalHubAccessTests
{
    private static readonly string ValidKey = new('x', 32);

    [Fact]
    public void AuthenticatedUserDoesNotNeedRobotKey()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "operator")], "test");

        Assert.True(SignalHubAccess.IsAllowed(new ClaimsPrincipal(identity), null, null));
        Assert.True(SignalHubAccess.IsOperator(new ClaimsPrincipal(identity)));
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

    [Fact]
    public void RobotPrincipalCannotActAsOperator()
    {
        var principal = SignalHubAccess.CreateRobotPrincipal();

        Assert.True(SignalHubAccess.IsRobot(principal));
        Assert.False(SignalHubAccess.IsOperator(principal));
    }

    [Fact]
    public void UnauthenticatedRobotClaimIsRejected()
    {
        var identity = new ClaimsIdentity(
            [new Claim(SignalHubAccess.ClientTypeClaim, SignalHubAccess.RobotClientType)]);

        Assert.False(SignalHubAccess.IsRobot(new ClaimsPrincipal(identity)));
    }

    [Fact]
    public void OperatorTokenRoundTripsAndRejectsTampering()
    {
        var service = new SignalHubTokenService(new EphemeralDataProtectionProvider());
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "operator-1")],
            "test");

        var token = service.CreateOperatorToken(new ClaimsPrincipal(identity));
        var principal = service.TryValidateOperatorToken(token);

        Assert.NotNull(principal);
        Assert.True(SignalHubAccess.IsOperator(principal));
        Assert.Equal("operator-1", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Null(service.TryValidateOperatorToken(token + "tampered"));
    }
}
