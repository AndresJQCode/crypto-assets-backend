using Api.Application.Dtos.Role;
using MediatR;

namespace Api.Application.Commands.RoleCommands;

internal sealed class CreateRoleCommand : IRequest<RoleDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IReadOnlyCollection<string>? PermissionIds { get; init; }
}
