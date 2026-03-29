using Application.Dtos.Permission;

namespace Application.Dtos.Auth;

internal sealed class AuthUserDto
{
    public string Id { get; set; } = string.Empty;
    public required string Email { get; set; }
    public required string Name { get; set; }
    public IReadOnlyCollection<string> Roles { get; init; } = [];
    public IReadOnlyCollection<UserPermissionDto> Permissions { get; init; } = [];
}
