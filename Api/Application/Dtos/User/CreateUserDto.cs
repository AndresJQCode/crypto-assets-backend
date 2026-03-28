namespace Api.Application.Dtos.User;

internal sealed class CreateUserDto
{
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required IReadOnlyCollection<string> Roles { get; init; }
}
