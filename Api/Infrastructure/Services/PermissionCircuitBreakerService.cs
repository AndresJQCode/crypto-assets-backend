using System.Data.Common;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Api.Infrastructure.Services;

internal interface IPermissionCircuitBreakerService
{
    Task<bool> ExecuteWithCircuitBreakerAsync<T>(Func<Task<T>> operation, T fallbackValue);
    Task<bool> ExecuteWithCircuitBreakerAsync(Func<Task<bool>> operation, bool fallbackValue = false);
    bool IsCircuitOpen { get; }
    string CircuitState { get; }
}

internal sealed class PermissionCircuitBreakerService(
    ILogger<PermissionCircuitBreakerService> logger,
    IOptionsMonitor<AppSettings> appSettings) : IPermissionCircuitBreakerService
{
    private readonly object _lock = new object();
    private DateTime _lastFailureTime = DateTime.MinValue;
    private int _failureCount;
    private bool _circuitOpen;

    private AppSettings.CircuitBreakerSettings Settings => appSettings.CurrentValue.CircuitBreaker;
    private TimeSpan BreakDuration => TimeSpan.FromSeconds(Settings.BreakDurationSeconds);
    private int FailureThreshold => Settings.FailureThreshold;
    private TimeSpan Timeout => TimeSpan.FromSeconds(Settings.TimeoutSeconds);


    public async Task<bool> ExecuteWithCircuitBreakerAsync<T>(Func<Task<T>> operation, T fallbackValue)
    {
        if (IsCircuitOpen)
        {
            logger.LogWarning("Circuit breaker abierto para operación de permisos. Usando valor de fallback: {FallbackValue}", fallbackValue);
            return fallbackValue is bool boolFallback ? boolFallback : false;
        }

        try
        {
            using var cts = new CancellationTokenSource(Timeout);
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("ExecuteWithCircuitBreakerAsync<T>: Ejecutando operación con timeout de {Timeout} segundos", Timeout.TotalSeconds);
            }

            // Aplicar el timeout a la operación usando WaitAsync
            var operationTask = operation();
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("ExecuteWithCircuitBreakerAsync<T>: Operación iniciada, esperando resultado con timeout");
            }

            var result = await operationTask.WaitAsync(cts.Token);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("ExecuteWithCircuitBreakerAsync<T>: Operación completada exitosamente");
            }

            // Éxito - resetear contador de fallos
            lock (_lock)
            {
                _failureCount = 0;
                _circuitOpen = false;
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Operación exitosa, circuit breaker cerrado");
            }

            // Si la operación devuelve un bool directamente
            if (result is bool boolResult)
            {
                return boolResult;
            }

            // Si la operación devuelve un valor no nulo, consideramos éxito
            return result != null;
        }
        catch (OperationCanceledException) when (IsCircuitOpen == false)
        {
            logger.LogWarning("Operación de permisos cancelada por timeout después de {Timeout} segundos", Timeout.TotalSeconds);

            lock (_lock)
            {
                _failureCount++;
                _lastFailureTime = DateTime.UtcNow;

                if (_failureCount >= FailureThreshold)
                {
                    _circuitOpen = true;
                    logger.LogWarning("Circuit breaker abierto debido a {FailureCount} fallos consecutivos (timeout)", _failureCount);
                }
            }

            return fallbackValue is bool boolFallback ? boolFallback : false;
        }
        catch (Exception ex) when (IsDatabaseException(ex))
        {
            logger.LogWarning(ex, "Error de base de datos detectado, incrementando contador de fallos");

            lock (_lock)
            {
                _failureCount++;
                _lastFailureTime = DateTime.UtcNow;

                if (_failureCount >= FailureThreshold)
                {
                    _circuitOpen = true;
                    logger.LogWarning("Circuit breaker abierto debido a {FailureCount} fallos consecutivos", _failureCount);
                }
            }

            return fallbackValue is bool boolFallback ? boolFallback : false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado en circuit breaker para operación de permisos");
            return fallbackValue is bool boolFallback ? boolFallback : false;
        }
    }

    public async Task<bool> ExecuteWithCircuitBreakerAsync(Func<Task<bool>> operation, bool fallbackValue = false)
    {
        var circuitState = CircuitState;
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("ExecuteWithCircuitBreakerAsync (bool): Estado del circuit breaker: {CircuitState}, IsOpen: {IsOpen}", circuitState, IsCircuitOpen);
        }

        if (IsCircuitOpen)
        {
            logger.LogWarning("Circuit breaker abierto para verificación de permisos. Usando valor de fallback: {FallbackValue}", fallbackValue);
            return fallbackValue;
        }

        try
        {
            using var cts = new CancellationTokenSource(Timeout);
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("ExecuteWithCircuitBreakerAsync (bool): Ejecutando operación con timeout de {Timeout} segundos", Timeout.TotalSeconds);
            }

            // Aplicar el timeout a la operación usando WaitAsync
            var operationTask = operation();
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("ExecuteWithCircuitBreakerAsync (bool): Operación iniciada, esperando resultado con timeout");
            }

            var result = await operationTask.WaitAsync(cts.Token);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("ExecuteWithCircuitBreakerAsync (bool): Operación completada exitosamente con resultado: {Result}", result);
            }

            // Éxito - resetear contador de fallos
            lock (_lock)
            {
                _failureCount = 0;
                _circuitOpen = false;
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Verificación de permisos exitosa, circuit breaker cerrado");
            }

            return result;
        }
        catch (OperationCanceledException) when (IsCircuitOpen == false)
        {
            logger.LogWarning("Verificación de permisos cancelada por timeout después de {Timeout} segundos", Timeout.TotalSeconds);

            lock (_lock)
            {
                _failureCount++;
                _lastFailureTime = DateTime.UtcNow;

                if (_failureCount >= FailureThreshold)
                {
                    _circuitOpen = true;
                    logger.LogWarning("Circuit breaker abierto debido a {FailureCount} fallos consecutivos (timeout)", _failureCount);
                }
            }

            return fallbackValue;
        }
        catch (Exception ex) when (IsDatabaseException(ex))
        {
            logger.LogWarning(ex, "Error de base de datos detectado, incrementando contador de fallos");

            lock (_lock)
            {
                _failureCount++;
                _lastFailureTime = DateTime.UtcNow;

                if (_failureCount >= FailureThreshold)
                {
                    _circuitOpen = true;
                    logger.LogWarning("Circuit breaker abierto debido a {FailureCount} fallos consecutivos", _failureCount);
                }
            }

            return fallbackValue;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado en circuit breaker para verificación de permisos");
            return fallbackValue;
        }
    }

    public bool IsCircuitOpen
    {
        get
        {
            lock (_lock)
            {
                // Si el circuito está abierto, verificar si ha pasado el tiempo de recuperación
                if (_circuitOpen && DateTime.UtcNow - _lastFailureTime > BreakDuration)
                {
                    _circuitOpen = false;
                    _failureCount = 0;
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("Circuit breaker cerrado después del período de recuperación");
                    }
                }

                return _circuitOpen;
            }
        }
    }

    public string CircuitState
    {
        get
        {
            return IsCircuitOpen ? "Open" : "Closed";
        }
    }

    private static bool IsDatabaseException(Exception ex)
    {
        return ex is DbException ||
               ex is TimeoutException ||
               ex is InvalidOperationException ||
               ex is HttpRequestException ||
               (ex.InnerException != null && IsDatabaseException(ex.InnerException));
    }
}
