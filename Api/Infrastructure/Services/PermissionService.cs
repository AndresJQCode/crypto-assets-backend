using Api.Application.Dtos.Permission;
using Api.Application.Queries.PermissionQueries;
using Domain.AggregatesModel.PermissionAggregate;
using Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Api.Infrastructure.Services;

internal sealed class PermissionService(IMediator mediator, IPermissionRepository permissionRepository, IPermissionCacheService cacheService, ILogger<PermissionService> logger) : IPermissionService
{
    public async Task<bool> HasPermissionAsync(Guid userId, string resource, string action)
    {
        // Intentar obtener del caché primero
        bool hasPermission = await cacheService.HasPermissionAsync(userId, resource, action);
        if (hasPermission)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Permission {Resource}.{Action} found in cache for user {UserId}", resource, action, userId);
            }

            return true;
        }

        // Si no está en caché, verificar en base de datos
        bool result = await mediator.Send(new CheckUserPermissionQuery(userId, resource, action));

        // Si tiene el permiso, actualizar el caché
        if (result)
        {
            IEnumerable<string> userPermissions = await GetUserPermissionsAsync(userId);
            await cacheService.CacheUserPermissionsAsync(userId, userPermissions);
        }

        return result;
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
    {
        // Intentar obtener del caché primero
        IEnumerable<string>? cachedPermissions = await cacheService.GetUserPermissionsAsync(userId);
        if (cachedPermissions.Any())
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("User permissions found in cache for user {UserId}", userId);
            }

            return cachedPermissions;
        }

        // Si no está en caché, obtener de base de datos
        IEnumerable<Permission>? permissions = await permissionRepository.GetPermissionsByUserIdAsync(userId);

        IEnumerable<UserPermissionDto>? permissionsDto = permissions.Select(p => new UserPermissionDto
        {
            PermissionKey = p.PermissionKey,
            Resource = p.Resource,
            Action = p.Action
        });

        List<string>? permissionKeys = permissionsDto.Select(p => p.PermissionKey)?.ToList() ?? [];

        // Guardar en caché
        await cacheService.CacheUserPermissionsAsync(userId, permissionKeys);

        return permissionKeys;
    }

    public async Task<bool> HasAnyPermissionAsync(Guid userId, params string[] permissionKeys)
    {
        var userPermissions = await GetUserPermissionsAsync(userId);
        return permissionKeys.Any(permissionKey => userPermissions.Contains(permissionKey));
    }

    public async Task<bool> HasAllPermissionsAsync(Guid userId, params string[] permissionKeys)
    {
        var userPermissions = await GetUserPermissionsAsync(userId);
        return permissionKeys.All(permissionKey => userPermissions.Contains(permissionKey));
    }

    public async Task InvalidateUserCacheAsync(Guid userId)
    {
        await cacheService.InvalidateUserCacheAsync(userId);
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Invalidated cache for user {UserId}", userId);
        }
    }

    public async Task InvalidateAllCacheAsync()
    {
        await cacheService.InvalidateAllCacheAsync();
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Invalidated all permission cache");
        }
    }
}
