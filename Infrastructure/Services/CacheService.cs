using System.Diagnostics;
using Domain.Interfaces;
using Infrastructure.Constants;
using Infrastructure.Metrics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService> _logger;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(15);

    public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public T? Get<T>(string key)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                stopwatch.Stop();
                InfrastructureMetrics.CacheHitsTotal.WithLabels(MetricsLabelsConstants.Cache.Memory, typeof(T).Name).Inc();
                InfrastructureMetrics.CacheOperationDuration.WithLabels(MetricsLabelsConstants.Cache.Get, MetricsLabelsConstants.Cache.Memory).Observe(stopwatch.Elapsed.TotalSeconds);
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                }

                return value;
            }

            stopwatch.Stop();
            InfrastructureMetrics.CacheMissesTotal.WithLabels(MetricsLabelsConstants.Cache.Memory, typeof(T).Name).Inc();
            InfrastructureMetrics.CacheOperationDuration.WithLabels("get", "memory").Observe(stopwatch.Elapsed.TotalSeconds);
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
            }

            return default;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
            return default;
        }
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration,
                SlidingExpiration = TimeSpan.FromMinutes(5),
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(key, value, cacheOptions);
            stopwatch.Stop();
            InfrastructureMetrics.CacheOperationDuration.WithLabels(MetricsLabelsConstants.Cache.Set, MetricsLabelsConstants.Cache.Memory).Observe(stopwatch.Elapsed.TotalSeconds);
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiration ?? _defaultExpiration);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
        }
    }

    public void Remove(string key)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _cache.Remove(key);
            stopwatch.Stop();
            InfrastructureMetrics.CacheOperationDuration.WithLabels(MetricsLabelsConstants.Cache.Remove, MetricsLabelsConstants.Cache.Memory).Observe(stopwatch.Elapsed.TotalSeconds);
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Removed cache entry for key: {Key}", key);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
        }
    }

    public void RemoveByPattern(string pattern)
    {
        try
        {
            if (_cache is MemoryCache memoryCache)
            {
                // En una implementación real, necesitarías mantener un registro de las claves
                // ya que IMemoryCache no tiene un método para buscar por patrón
                _logger.LogWarning("RemoveByPattern not fully implemented for pattern: {Pattern}", pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache values by pattern: {Pattern}", pattern);
        }
    }

    public bool Exists(string key)
    {
        try
        {
            return _cache.TryGetValue(key, out _);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
            return false;
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        try
        {
            var cachedValue = Get<T>(key);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            var value = await factory();
            Set(key, value, expiration);
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrSetAsync for key: {Key}", key);
            return await factory();
        }
    }
}
