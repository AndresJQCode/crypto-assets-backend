namespace Infrastructure.Constants;

/// <summary>
/// Permission resource names. Single source of truth for authorization and seed data.
/// </summary>
public static class PermissionResourcesConstants
{
    public const string Users = nameof(Users);
    public const string Roles = nameof(Roles);
    public const string Permissions = nameof(Permissions);
    public const string Dashboard = nameof(Dashboard);
    public const string Tenants = nameof(Tenants);
    public const string ConnectorDefinitions = nameof(ConnectorDefinitions);
    public const string SystemConfiguration = nameof(SystemConfiguration);
    public const string ConnectorInstances = nameof(ConnectorInstances);
}

/// <summary>
/// Permission action types. Single source of truth for authorization and seed data.
/// </summary>
public static class PermissionActionsConstants
{
    public const string Create = nameof(Create);
    public const string Read = nameof(Read);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string List = nameof(List);
    public const string Assign = nameof(Assign);
    public const string Activate = nameof(Activate);
    public const string Enable = nameof(Enable);
    public const string Reauthorize = nameof(Reauthorize);
    public const string Validate = nameof(Validate);
}
