namespace Api.Application.Dtos.Role;

internal sealed class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IReadOnlyCollection<string>? PermissionIds { get; init; }
}
