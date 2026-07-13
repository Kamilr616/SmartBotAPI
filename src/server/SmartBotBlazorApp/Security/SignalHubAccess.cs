using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartBotBlazorApp;

public static class SignalHubAccess
{
    public const int MinimumApiKeyLength = 32;

    public static bool IsAllowed(ClaimsPrincipal user, string? expectedApiKey, string? suppliedApiKey)
    {
        if (user.Identity?.IsAuthenticated == true)
        {
            return true;
        }

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
}
