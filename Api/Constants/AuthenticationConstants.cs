namespace Api.Constants;

/// <summary>
/// Constants for authentication and authorization used throughout the application.
/// </summary>
internal static class AuthenticationConstants
{
    /// <summary>
    /// Authentication schemes
    /// </summary>
    internal static class Schemes
    {
        public const string Bearer = "Bearer";
    }

    /// <summary>
    /// Policy names for rate limiting and authorization
    /// </summary>
    internal static class Policies
    {
        public const string AuthPolicy = nameof(AuthPolicy);
        public const string GeneralPolicy = nameof(GeneralPolicy);
    }
}
