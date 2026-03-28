using Api.Application.Services;
using Infrastructure.Services;
using MediatR;

namespace Api.Application.Queries.PermissionQueries;

internal sealed class CheckUserPermissionQueryHandler(
    IUserPermissionService userPermissionService,
    IPermissionCacheService permissionCacheService,
    ILogger<CheckUserPermissionQueryHandler> logger) : IRequestHandler<CheckUserPermissionQuery, bool>
{
    public async Task<bool> Handle(CheckUserPermissionQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("CheckUserPermissionQueryHandler: Iniciando verificación de permiso para usuario {UserId}, recurso: {Resource}, acción: {Action}",
            request.UserId, request.Resource, request.Action);

        try
        {
            // Intentar obtener el resultado del caché primero
            var cachedResult = await permissionCacheService.HasPermissionAsync(request.UserId, request.Resource, request.Action);

            logger.LogDebug("CheckUserPermissionQueryHandler: Resultado del caché: {CachedResult}", cachedResult);

            // Si el caché tiene datos, devolver el resultado
            if (cachedResult)
            {
                return true;
            }

            // Si no está en caché, verificar en la base de datos
            bool hasPermission = await userPermissionService.UserHasPermissionAsync(
                request.UserId,
                $"{request.Resource}.{request.Action}",
                cancellationToken);

            // Si el usuario tiene el permiso, cachear todos sus permisos para futuras consultas
            if (hasPermission)
            {
                var userPermissions = await userPermissionService.GetUserPermissionsAsync(request.UserId, cancellationToken);
                var permissionKeys = userPermissions.Select(p => p.PermissionKey).ToList();
                await permissionCacheService.CacheUserPermissionsAsync(request.UserId, permissionKeys);
            }

            return hasPermission;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking permission for user {UserId}, resource: {Resource}, action: {Action}",
                request.UserId, request.Resource, request.Action);

            // En caso de error, hacer fallback a la consulta directa sin caché
            return await userPermissionService.UserHasPermissionAsync(
                request.UserId,
                $"{request.Resource}.{request.Action}",
                cancellationToken);
        }
    }
}
