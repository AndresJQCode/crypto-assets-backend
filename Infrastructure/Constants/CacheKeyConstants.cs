using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Constants;

/// <summary>
/// Constants for cache keys and prefixes used throughout the application.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Nested constant classes are intentionally public for organization")]
public static class CacheKeyConstants
{
    /// <summary>
    /// Password reset related cache keys
    /// </summary>
    public static class PasswordReset
    {
        public const string TrackerPrefix = "password_reset_tracker_";

        /// <summary>
        /// Generates a cache key for password reset tracker by email
        /// </summary>
        public static string GetTrackerKey(string email) => $"{TrackerPrefix}{email.ToUpperInvariant()}";
    }

    /// <summary>
    /// User permissions cache keys
    /// </summary>
    public static class Permissions
    {
        public const string UserPermissionsPrefix = "user_permissions_";

        /// <summary>
        /// Generates a cache key for user permissions
        /// </summary>
        public static string GetUserPermissionsKey(Guid userId) => $"{UserPermissionsPrefix}{userId}";
    }
}
