using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Constants;

/// <summary>
/// Constants for configuration section keys used throughout the application.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Nested constant classes are intentionally public for organization")]
public static class ConfigurationKeyConstants
{
    public const string Authentication = nameof(Authentication);
    public const string JwtSettings = nameof(JwtSettings);
    public const string OpenApi = nameof(OpenApi);
    public const string Recaptcha = nameof(Recaptcha);
    public const string RateLimiting = nameof(RateLimiting);
    public const string DB = nameof(DB);
    public const string OAuth = nameof(OAuth);
    public const string EmailSettings = nameof(EmailSettings);
    public const string PermissionMiddleware = nameof(PermissionMiddleware);
}
