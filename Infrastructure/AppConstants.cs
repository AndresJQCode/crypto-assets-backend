using System.Diagnostics.CodeAnalysis;

namespace Infrastructure
{
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Nested constant classes are intentionally public for organization")]
    public static class AppConstants
    {
        public static class Authentication
        {
            public const string DefaultProvider = "Default";
            public const string AccessTokenName = "accessToken";
            public const string RefreshTokenName = "refreshToken";
            /// <summary>Claim para el tenant del usuario en el JWT. Null/empty = SuperAdmin (plataforma).</summary>
            public const string TenantIdClaim = "tenant_id";
        }

        public static class AuditEntityTypes
        {
            public const string Authentication = "Authentication";
            public const string User = "User";
            public const string Role = "Role";
            public const string Permission = "Permission";
        }

        public static class AuditActions
        {
            // Acciones de autenticación
            public const string UserRegistered = "USER_REGISTERED";
            public const string LoginFailed = "LOGIN_FAILED";
            public const string PasswordResetRequested = "PASSWORD_RESET_REQUESTED";
            public const string PasswordChanged = "PASSWORD_CHANGED";

            // Acciones de permisos
            public const string PermissionsUpdated = "PERMISSIONS_UPDATED";

            // Acciones de eliminación/restauración
            public const string Delete = "DELETE";
            public const string Restore = "RESTORE";
            public const string HardDelete = "HARD_DELETE";
        }
    }
}
