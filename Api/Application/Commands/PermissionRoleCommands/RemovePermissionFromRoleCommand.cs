using MediatR;

namespace Api.Application.Commands.PermissionRoleCommands;

internal sealed class RemovePermissionFromRoleCommand(Guid permissionId, Guid roleId) : IRequest<bool>
{
    public Guid PermissionId { get; private set; } = permissionId;
    public Guid RoleId { get; private set; } = roleId;
}
