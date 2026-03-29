namespace Application.Dtos.User;

internal sealed class UpdateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IReadOnlyCollection<string>? RoleIds { get; init; }
}
