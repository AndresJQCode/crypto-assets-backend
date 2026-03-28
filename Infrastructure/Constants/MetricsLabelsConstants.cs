using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Constants;

/// <summary>
/// Constants for Prometheus metric labels used throughout the application.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Nested constant classes are intentionally public for organization")]
public static class MetricsLabelsConstants
{
    /// <summary>
    /// Cache operation labels
    /// </summary>
    public static class Cache
    {
        public const string PermissionCache = "permission_cache";
        public const string Memory = "memory";
        public const string Get = "get";
        public const string Set = "set";
        public const string Remove = "remove";
        public const string UserPermissions = "user_permissions";
    }

    /// <summary>
    /// OAuth provider labels
    /// </summary>
    public static class OAuth
    {
        public const string GoogleOAuth = "google_oauth";
        public const string MicrosoftOAuth = "microsoft_oauth";
        public const string ExchangeError = "exchange_error";
        public const string ExchangeSuccess = "exchange_success";
        public const string ExchangeFailed = "exchange_failed";
        public const string ConfigError = "config_error";
        public const string UserInfoError = "userinfo_error";
        public const string UserInfoSuccess = "userinfo_success";
        public const string UserInfoFailed = "userinfo_failed";
    }

    /// <summary>
    /// JWT token operation labels
    /// </summary>
    public static class Jwt
    {
        public const string Access = "access";
        public const string Refresh = "refresh";
        public const string ValidationSuccess = "validation_success";
        public const string ValidationFailed = "validation_failed";
        public const string RefreshValidationInvalidType = "refresh_validation_invalid_type";
        public const string RefreshValidationSuccess = "refresh_validation_success";
        public const string RefreshValidationFailed = "refresh_validation_failed";
    }

    /// <summary>
    /// Middleware operation labels
    /// </summary>
    public static class Middleware
    {
        public const string Label = "middleware";
        public const string NotAuthenticated = "not_authenticated";
        public const string InvalidClaim = "invalid_claim";
        public const string PermissionService = "permission-service";
        public const string PermissionDenied = "permission_denied";
    }

    /// <summary>
    /// Database operation labels
    /// </summary>
    public static class Database
    {
        // Operation types
        public const string Insert = "insert";
        public const string InsertRange = "insert_range";
        public const string Update = "update";
        public const string Delete = "delete";
        public const string Query = "query";
        public const string Select = "select";
        public const string SelectFirst = "select_first";
        public const string SelectById = "select_by_id";
        public const string SelectProjection = "select_projection";
        public const string SelectFirstProjection = "select_first_projection";
        public const string SelectPaginated = "select_paginated";
        public const string BulkDelete = "bulk_delete";
        public const string BulkUpdate = "bulk_update";
        public const string SpecificationSelect = "specification_select";
        public const string SpecificationSelectFirst = "specification_select_first";
        public const string SpecificationCount = "specification_count";
        public const string SpecificationPaginated = "specification_paginated";

        // Status labels
        public const string Success = "success";
        public const string Error = "error";
    }

    /// <summary>
    /// Authentication operation labels
    /// </summary>
    public static class Authentication
    {
        public const string Login = "login";
        public const string Register = "register";
        public const string Success = "success";
        public const string Failed = "failed";
    }
}
