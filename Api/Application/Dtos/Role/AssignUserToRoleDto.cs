namespace Api.Application.Dtos.Role;

internal sealed class AssignUserToRoleDto
{
    public string UserId { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
}
