using Api.Application.Dtos.Permission;
using MediatR;

namespace Api.Application.Commands.RoleCommands;

internal sealed class UpdateRoleWithPermissionsCommand : IRequest<UpdateRoleWithPermissionsResponse>
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public IReadOnlyCollection<Guid>? PermissionIds { get; init; }
}

internal sealed class UpdateRoleWithPermissionsResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IReadOnlyCollection<PermissionDto> Permissions { get; init; } = [];
}

