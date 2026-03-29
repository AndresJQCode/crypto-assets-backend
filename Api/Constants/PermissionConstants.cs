namespace Api.Constants;

/// <summary>
/// Constants for permission-based authorization system.
/// Contains resource names and action types used throughout the application.
/// </summary>
internal static class PermissionConstants
{
    /// <summary>
    /// Permission resource names
    /// </summary>
    internal static class Resources
    {
        public const string Users = nameof(Users);
        public const string Roles = nameof(Roles);
        public const string Permissions = nameof(Permissions);
        public const string Dashboard = nameof(Dashboard);
    }

    /// <summary>
    /// Permission action types
    /// </summary>
    internal static class Actions
    {
        public const string Create = nameof(Create);
        public const string Read = nameof(Read);
        public const string Update = nameof(Update);
        public const string Delete = nameof(Delete);
        public const string List = nameof(List);
    }
}
