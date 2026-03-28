using Api.Application.Dtos.Permission;

namespace Api.Application.Dtos.Role;

internal sealed class RoleWithPermissionsDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IReadOnlyCollection<PermissionDto> Permissions { get; init; } = [];
}
