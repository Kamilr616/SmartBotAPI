using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace SmartBotBlazorApp;

public sealed class SignalHubTokenService
{
    private static readonly TimeSpan OperatorTokenLifetime = TimeSpan.FromMinutes(5);
    private readonly ITimeLimitedDataProtector _protector;

    public SignalHubTokenService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider
            .CreateProtector("SmartBotAPI.SignalHub.OperatorToken.v1")
            .ToTimeLimitedDataProtector();
    }

    public string CreateOperatorToken(ClaimsPrincipal user)
    {
        if (!SignalHubAccess.IsOperator(user))
        {
            throw new InvalidOperationException("Only an authenticated dashboard user can receive an operator token.");
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("The authenticated dashboard user has no stable identifier.");
        }

        return _protector.Protect(userId, OperatorTokenLifetime);
    }

    public ClaimsPrincipal? TryValidateOperatorToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var userId = _protector.Unprotect(token, out _);
            var identity = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, userId),
                    new Claim(SignalHubAccess.ClientTypeClaim, SignalHubAccess.OperatorClientType)
                ],
                authenticationType: "SignalHubOperatorToken");
            return new ClaimsPrincipal(identity);
        }
        catch (CryptographicException)
        {
            return null;
        }
    }
}
