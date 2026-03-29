using Application.Dtos.Permission;

namespace Application.Dtos.User;

internal sealed class CurrentUserDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyCollection<string> Roles { get; init; } = [];
    public IReadOnlyCollection<UserPermissionDto> Permissions { get; init; } = [];
}
