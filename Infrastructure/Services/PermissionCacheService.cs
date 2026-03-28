using System.Diagnostics;
using Infrastructure.Constants;
using Infrastructure.Metrics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public interface IPermissionCacheService
{
    Task<bool> HasPermissionAsync(Guid userId, string resource, string action);
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId);
    Task InvalidateUserCacheAsync(Guid userId);
    Task InvalidateAllCacheAsync();
    Task CacheUserPermissionsAsync(Guid userId, IEnumerable<string> permissions);
}

public class PermissionCacheService(
    IMemoryCache cache,
    ILogger<PermissionCacheService> logger,
    IOptionsMonitor<PermissionCacheOptions> options) : IPermissionCacheService
{
    private PermissionCacheOptions Options => options.CurrentValue;

    public Task<bool> HasPermissionAsync(Guid userId, string resource, string action)
    {
        // Validaciones de entrada
        if (userId == Guid.Empty)
        {
            logger.LogWarning("HasPermissionAsync called with empty userId");
            return Task.FromResult(false);
        }

        if (string.IsNullOrWhiteSpace(resource) || string.IsNullOrWhiteSpace(action))
        {
            logger.LogWarning("HasPermissionAsync called with invalid resource or action: {Resource}, {Action}", resource, action);
            return Task.FromResult(false);
        }

        var permissionKey = $"{resource}.{action}";
        var userPermissionsKey = GetUserPermissionsKey(userId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (cache.TryGetValue(userPermissionsKey, out IEnumerable<string>? cachedPermissions))
            {
                stopwatch.Stop();

                // Métrica de Prometheus: permiso encontrado en caché
                InfrastructureMetrics.PermissionCacheHitsTotal
                    .WithLabels(MetricsLabelsConstants.Cache.PermissionCache, MetricsLabelsConstants.Cache.UserPermissions)
                    .Inc();

                InfrastructureMetrics.PermissionCacheOperationDuration
                    .WithLabels(MetricsLabelsConstants.Cache.Get, MetricsLabelsConstants.Cache.PermissionCache)
                    .Observe(stopwatch.Elapsed.TotalSeconds);

                if (Options.EnableDetailedLogging)
                {
                    logger.LogDebug("Cache hit for user {UserId} permissions", userId);
                }
                return Task.FromResult(cachedPermissions?.Contains(permissionKey) ?? false);
            }

            stopwatch.Stop();

            // Métrica de Prometheus: permiso NO encontrado en caché
            InfrastructureMetrics.PermissionCacheMissesTotal
                .WithLabels("permission_cache", "user_permissions")
                .Inc();

            InfrastructureMetrics.PermissionCacheOperationDuration
                .WithLabels("get", "permission_cache")
                .Observe(stopwatch.Elapsed.TotalSeconds);

            if (Options.EnableDetailedLogging)
            {
                logger.LogDebug("Cache miss for user {UserId} permissions", userId);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking permission in cache for user {UserId}, resource: {Resource}, action: {Action}",
                userId, resource, action);
            return Task.FromResult(false);
        }
    }

    public Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
    {
        // Validaciones de entrada
        if (userId == Guid.Empty)
        {
            logger.LogWarning("GetUserPermissionsAsync called with empty userId");
            return Task.FromResult<IEnumerable<string>>([]);
        }

        var userPermissionsKey = GetUserPermissionsKey(userId);

        try
        {
            if (cache.TryGetValue(userPermissionsKey, out IEnumerable<string>? cachedPermissions))
            {
                // Métrica de Prometheus: permiso encontrado en caché
                InfrastructureMetrics.PermissionCacheHitsTotal
                    .WithLabels(MetricsLabelsConstants.Cache.PermissionCache, MetricsLabelsConstants.Cache.UserPermissions)
                    .Inc();

                if (Options.EnableDetailedLogging)
                {
                    logger.LogDebug("Cache hit for user {UserId} permissions", userId);
                }
                return Task.FromResult(cachedPermissions ?? []);
            }

            // Métrica de Prometheus: permiso NO encontrado en caché
            InfrastructureMetrics.PermissionCacheMissesTotal
                .WithLabels("permission_cache", "user_permissions")
                .Inc();

            if (Options.EnableDetailedLogging)
            {
                logger.LogDebug("Cache miss for user {UserId} permissions", userId);
            }
            return Task.FromResult<IEnumerable<string>>([]);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user permissions from cache for user {UserId}", userId);
            return Task.FromResult<IEnumerable<string>>([]);
        }
    }

    public Task CacheUserPermissionsAsync(Guid userId, IEnumerable<string> permissions)
    {
        // Validaciones de entrada
        if (userId == Guid.Empty)
        {
            logger.LogWarning("CacheUserPermissionsAsync called with empty userId");
            return Task.CompletedTask;
        }

        if (permissions == null)
        {
            logger.LogWarning("CacheUserPermissionsAsync called with null permissions for user {UserId}", userId);
            return Task.CompletedTask;
        }

        var userPermissionsKey = GetUserPermissionsKey(userId);
        var permissionsList = permissions.ToList();

        try
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = Options.AbsoluteExpiration,
                SlidingExpiration = Options.SlidingExpiration,
                Priority = Options.Priority,
                // Establecer tamaño para compatibilidad con SizeLimit del MemoryCache
                // Cada permiso cuenta como 1 unidad de tamaño
                Size = Math.Max(1, permissionsList.Count),

                // Callbacks para eventos de evicción
                PostEvictionCallbacks = {
                    new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = (key, value, reason, state) =>
                        {
                            if (Options.EnableDetailedLogging)
                            {
                                logger.LogDebug("Permission cache evicted for user {UserId}, reason: {Reason}",
                                    userId, reason);
                            }
                        }
                    }
                }
            };

            cache.Set(userPermissionsKey, permissionsList, cacheOptions);

            if (Options.EnableDetailedLogging)
            {
                logger.LogDebug("Cached {Count} permissions for user {UserId}", permissionsList.Count, userId);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to cache permissions for user {UserId}", userId);
            throw;
        }
    }

    public Task InvalidateUserCacheAsync(Guid userId)
    {
        // Validaciones de entrada
        if (userId == Guid.Empty)
        {
            logger.LogWarning("InvalidateUserCacheAsync called with empty userId");
            return Task.CompletedTask;
        }

        try
        {
            var userPermissionsKey = GetUserPermissionsKey(userId);
            cache.Remove(userPermissionsKey);

            if (Options.EnableDetailedLogging)
            {
                logger.LogDebug("Invalidated cache for user {UserId}", userId);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to invalidate cache for user {UserId}", userId);
            throw;
        }
    }

    public Task InvalidateAllCacheAsync()
    {
        try
        {
            if (cache is MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0); // Remove all entries
                logger.LogInformation("Invalidated all permission cache");
            }
            else
            {
                logger.LogWarning("Cache is not a MemoryCache instance, cannot invalidate all entries");
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to invalidate all cache");
            throw;
        }
    }

    private static string GetUserPermissionsKey(Guid userId) => $"user_permissions_{userId}";
}
