using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Infrastructure.HealthChecks;

internal sealed class MemoryCacheHealthCheck : IHealthCheck
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheHealthCheck> _logger;

    public MemoryCacheHealthCheck(IMemoryCache memoryCache, ILogger<MemoryCacheHealthCheck> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificación simple: intentar escribir y leer una clave de prueba
            var testKey = $"health_check_{Guid.NewGuid()}";
            var testValue = "test";

            _memoryCache.Set(testKey, testValue, TimeSpan.FromSeconds(5));
            var retrievedValue = _memoryCache.Get<string>(testKey);
            _memoryCache.Remove(testKey);

            if (retrievedValue != testValue)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Caché no funciona"));
            }

            return Task.FromResult(HealthCheckResult.Healthy("Caché disponible"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar caché de memoria");
            return Task.FromResult(HealthCheckResult.Unhealthy("Error en caché", exception: ex));
        }
    }
}

