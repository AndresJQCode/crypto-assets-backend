using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services;

/// <summary>
/// Opciones de configuración para el servicio de caché de permisos
/// </summary>
public class PermissionCacheOptions
{
    /// <summary>
    /// Tiempo de expiración absoluta del caché
    /// </summary>
    public TimeSpan AbsoluteExpiration { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Tiempo de expiración deslizante del caché
    /// </summary>
    public TimeSpan SlidingExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Prioridad de los elementos en el caché
    /// </summary>
    public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;

    /// <summary>
    /// Tamaño máximo del caché (número de usuarios)
    /// </summary>
    public int MaxCacheSize { get; set; } = 1000;

    /// <summary>
    /// Habilitar logging detallado de operaciones de caché
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;
}
