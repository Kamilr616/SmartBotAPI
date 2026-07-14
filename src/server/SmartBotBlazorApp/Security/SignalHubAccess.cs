using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartBotBlazorApp;

public static class SignalHubAccess
{
    public const int MinimumApiKeyLength = 32;
    public const string ClientTypeClaim = "smartbot:client_type";
    public const string OperatorClientType = "operator";
    public const string RobotClientType = "robot";
    private const string RobotAuthenticationType = "RobotApiKey";

    public static bool IsAllowed(ClaimsPrincipal user, string? expectedApiKey, string? suppliedApiKey)
    {
        return IsOperator(user) || IsRobot(user) || HasValidRobotKey(expectedApiKey, suppliedApiKey);
    }

    public static bool IsOperator(ClaimsPrincipal? user)
    {
        return user?.Identity?.IsAuthenticated == true && !IsRobot(user);
    }

    public static bool IsRobot(ClaimsPrincipal? user)
    {
        return user?.Identities.Any(identity =>
            identity.IsAuthenticated &&
            identity.AuthenticationType == RobotAuthenticationType &&
            identity.HasClaim(ClientTypeClaim, RobotClientType)) == true;
    }

    public static bool HasValidRobotKey(string? expectedApiKey, string? suppliedApiKey)
    {
        if (string.IsNullOrEmpty(expectedApiKey) ||
            expectedApiKey.Length < MinimumApiKeyLength ||
            string.IsNullOrEmpty(suppliedApiKey))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(expectedApiKey);
        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedApiKey);
        return expectedBytes.Length == suppliedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes);
    }

    public static ClaimsPrincipal CreateRobotPrincipal()
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClientTypeClaim, RobotClientType)],
            authenticationType: RobotAuthenticationType);
        return new ClaimsPrincipal(identity);
    }
}
