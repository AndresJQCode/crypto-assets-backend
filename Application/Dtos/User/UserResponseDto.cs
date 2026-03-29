namespace Application.Dtos.User;

internal sealed class UserResponseDto
{
    public string Id { get; set; } = string.Empty;
    public required string Email { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public UserRoleDto[] Roles { get; set; } = [];
}
