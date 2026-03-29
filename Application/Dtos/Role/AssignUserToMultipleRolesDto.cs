namespace Application.Dtos.Role;

internal sealed class AssignUserToMultipleRolesDto
{
    public string UserId { get; set; } = string.Empty;
    public IReadOnlyCollection<string> RoleIds { get; init; } = [];
}
