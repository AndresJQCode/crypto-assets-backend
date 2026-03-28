namespace Api.Constants;

/// <summary>
/// Constants for route paths used throughout the application.
/// </summary>
internal static class RouteConstants
{
    /// <summary>
    /// Health check related routes
    /// </summary>
    internal static class Health
    {
        public const string Base = "/health";
        public const string Live = "/health/live";
        public const string Ready = "/health/ready";
        public const string Database = "/health/db";
    }

    /// <summary>
    /// Authentication related routes
    /// </summary>
    internal static class Auth
    {
        public const string Base = "/auth";
        public const string Login = "/auth/login";
        public const string Register = "/auth/register";
        public const string ExchangeCode = "/auth/exchangeCode";
        public const string Refresh = "/auth/refresh";
        public const string ForgotPassword = "/auth/forgotPassword";
        public const string Me = "/auth/me";
        public const string Logout = "/auth/logout";
    }

    /// <summary>
    /// Monitoring and observability routes
    /// </summary>
    internal static class Monitoring
    {
        public const string Metrics = "/metrics";
    }

    /// <summary>
    /// API documentation routes
    /// </summary>
    internal static class Documentation
    {
        public const string Swagger = "/swagger";
        public const string Scalar = "/scalar";
        public const string OpenApiJson = "/openapi/v1.json";
    }
}
