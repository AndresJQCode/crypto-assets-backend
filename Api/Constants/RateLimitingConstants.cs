namespace Api.Constants;

/// <summary>
/// Constants for rate limiting configuration used throughout the application.
/// </summary>
internal static class RateLimitingConstants
{
    /// <summary>
    /// Partition key patterns for rate limiting
    /// </summary>
    internal static class PartitionKeys
    {
        public const string Excluded = "excluded";
        public const string Global = "global";
        public const string Auth = "auth";
        public const string General = "general";

        /// <summary>
        /// Format: "{policyType}:user:{userId}"
        /// </summary>
        public const string UserPattern = "{0}:user:{1}";

        /// <summary>
        /// Format: "{policyType}:ip:{ipAddress}"
        /// </summary>
        public const string IpPattern = "{0}:ip:{1}";
    }
}
