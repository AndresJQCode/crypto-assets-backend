namespace Api.Constants;

/// <summary>
/// Constants for HTTP headers used throughout the application.
/// </summary>
internal static class HeaderConstants
{
    /// <summary>
    /// Correlation ID header for request tracing
    /// </summary>
    public const string CorrelationId = "X-Correlation-ID";

    /// <summary>
    /// Pagination headers
    /// </summary>
    internal static class Pagination
    {
        public const string Page = "X-Page";
        public const string Limit = "X-Limit";
        public const string Search = "X-Search";
        public const string SortBy = "X-Sort-By";
        public const string SortOrder = "X-Sort-Order";
        public const string TotalCount = "X-Total-Count";
        public const string TotalPages = "X-Total-Pages";
    }

    /// <summary>
    /// IP address headers
    /// </summary>
    internal static class IpAddress
    {
        public const string ForwardedFor = "X-Forwarded-For";
        public const string RealIp = "X-Real-IP";
    }

    /// <summary>
    /// Rate limiting headers
    /// </summary>
    internal static class RateLimiting
    {
        public const string RetryAfter = "Retry-After";
    }

    /// <summary>
    /// Content type headers
    /// </summary>
    internal static class ContentType
    {
        public const string ApplicationJson = "application/json";
    }
}
