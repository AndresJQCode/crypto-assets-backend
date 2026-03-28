using Api.Infrastructure.Services;
using Domain.AggregatesModel.PermissionAggregate;
using Domain.AggregatesModel.UserAggregate;
using MediatR;

namespace Api.Application.Commands.PermissionRoleCommands;

internal sealed class RemovePermissionFromRoleCommandHandler(
    IPermissionRoleRepository permissionRoleRepository,
    IPermissionService permissionService,
    IUserRoleRepository userRoleRepository
) : IRequestHandler<RemovePermissionFromRoleCommand, bool>
{

    public async Task<bool> Handle(RemovePermissionFromRoleCommand request, CancellationToken cancellationToken)
    {
        PermissionRole? permissionRole = await permissionRoleRepository.GetByRoleAndPermissionAsync(request.RoleId, request.PermissionId, cancellationToken);

        if (permissionRole is null)
        {
            return false;
        }

        bool result = permissionRoleRepository.Delete(permissionRole);
        if (!result)
        {
            return false;
        }

        await permissionRoleRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        // Invalidar caché de permisos para usuarios que tienen este rol
        await InvalidateCacheForRoleUsers(request.RoleId, cancellationToken);

        return result;
    }

    private async Task InvalidateCacheForRoleUsers(Guid roleId, CancellationToken cancellationToken)
    {
        try
        {
            var userIds = await userRoleRepository.GetUserIdsByRoleIdAsync(roleId, cancellationToken);

            foreach (var userId in userIds)
            {
                await permissionService.InvalidateUserCacheAsync(userId);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al invalidar caché para usuarios del rol {roleId}: {ex.Message}", ex);
        }
    }
}
