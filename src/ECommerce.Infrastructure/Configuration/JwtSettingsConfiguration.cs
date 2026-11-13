using System;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Infrastructure.Configuration;

public readonly record struct JwtSettingsValues(
    string Key,
    string Issuer,
    string Audience,
    int ExpiresInMinutes);

public static class JwtSettingsConfiguration
{
    public const string SectionName = "JwtSettings";
    public const int MinimumKeyLengthBytes = 32;

    public static JwtSettingsValues GetRequired(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);
        var key = section["Key"];
        var issuer = section["Issuer"];
        var audience = section["Audience"];
        var expiresRaw = section["ExpiresInMinutes"];

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "JWT signing key is not configured (JwtSettings:Key). " +
                "From src/ECommerce.Api, set a user secret: " +
                "dotnet user-secrets set \"JwtSettings:Key\" \"<at-least-32-bytes>\". " +
                "Or set the JwtSettings__Key environment variable. Do not commit the key.");
        }

        if (Encoding.UTF8.GetByteCount(key) < MinimumKeyLengthBytes)
        {
            throw new InvalidOperationException(
                $"JWT signing key (JwtSettings:Key) must be at least {MinimumKeyLengthBytes} bytes for HMAC-SHA256. " +
                "Set a longer value with user secrets or the JwtSettings__Key environment variable.");
        }

        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException("JwtSettings:Issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("JwtSettings:Audience is not configured.");
        }

        if (!int.TryParse(expiresRaw, out var expiresInMinutes) || expiresInMinutes <= 0)
        {
            throw new InvalidOperationException(
                "JwtSettings:ExpiresInMinutes must be a positive integer.");
        }

        return new JwtSettingsValues(key, issuer, audience, expiresInMinutes);
    }
}
