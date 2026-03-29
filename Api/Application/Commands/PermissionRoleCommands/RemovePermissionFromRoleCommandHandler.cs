using Api.Infrastructure.Services;
using Domain.AggregatesModel.PermissionAggregate;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Commands.PermissionRoleCommands;

internal sealed class RemovePermissionFromRoleCommandHandler(
    IPermissionRoleRepository permissionRoleRepository,
    IPermissionService permissionService,
    ApiContext context
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
            // Obtener usuarios que tienen este rol específico usando consulta directa
            var userIds = await context.UserRoles
                .Where(ur => ur.RoleId == roleId)
                .Select(ur => ur.UserId)
                .ToListAsync(cancellationToken);

            // Invalidar caché solo para estos usuarios
            foreach (var userId in userIds)
            {
                await permissionService.InvalidateUserCacheAsync(userId);
            }
        }
        catch (Exception ex)
        {
            // Log del error pero no fallar la operación principal
            // En un escenario real, podrías usar un logger aquí
            throw new InvalidOperationException($"Error al invalidar caché para usuarios del rol {roleId}: {ex.Message}", ex);
        }
    }
}
