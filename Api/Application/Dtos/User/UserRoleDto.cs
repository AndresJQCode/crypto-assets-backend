namespace Api.Application.Dtos.User;

internal sealed class UserRoleDto
{
    public string Id { get; set; } = string.Empty;
    public required string Name { get; set; }
}
