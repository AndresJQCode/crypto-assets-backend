namespace Api.Application.Dtos.Permission;

internal sealed class UserPermissionDto
{
    public required string PermissionKey { get; set; } // Formato: "Resource.Action"
    public required string Resource { get; set; }
    public required string Action { get; set; }
}
