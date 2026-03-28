using System.Diagnostics;
using Api.Constants;
using Api.Infrastructure.Metrics;
using Domain.Exceptions;

namespace Api.Infrastructure.Middlewares;

/// <summary>
/// Middleware para capturar métricas HTTP automáticamente en Prometheus
/// </summary>
internal sealed class PrometheusMetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PrometheusMetricsMiddleware> _logger;

    public PrometheusMetricsMiddleware(RequestDelegate next, ILogger<PrometheusMetricsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // No capturar métricas de los propios endpoints de métricas
        if (context.Request.Path.StartsWithSegments(RouteConstants.Monitoring.Metrics, StringComparison.Ordinal))
        {
            await _next(context);
            return;
        }

        var method = context.Request.Method;
        var endpoint = GetEndpointPath(context);

        // Incrementar contador de requests en progreso
        ApiMetrics.HttpRequestsInProgress.WithLabels(method).Inc();

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Las excepciones de negocio esperadas (BadRequestException, NotFoundException, etc.)
            // no deben registrarse como excepciones no manejadas, ya que son errores de validación/negocio esperados
            // Solo registramos excepciones inesperadas del sistema como no manejadas
            if (ex is not (BadRequestException or NotFoundException or UnAuthorizedException or DomainException))
            {
                // Registrar excepción no manejada solo para excepciones inesperadas
                ApiMetrics.UnhandledExceptionsTotal
                    .WithLabels(ex.GetType().Name, endpoint)
                    .Inc();

                _logger.LogError(ex, "Excepción no manejada en {Endpoint}", endpoint);
            }

            throw;
        }
        finally
        {
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var duration = stopwatch.Elapsed.TotalSeconds;

            // Decrementar contador de requests en progreso
            ApiMetrics.HttpRequestsInProgress.WithLabels(method).Dec();

            // Registrar duración del request
            ApiMetrics.HttpRequestDuration
                .WithLabels(method, endpoint)
                .Observe(duration);

            // Incrementar contador total de requests
            ApiMetrics.HttpRequestsTotal
                .WithLabels(method, endpoint, statusCode)
                .Inc();
        }
    }

    private static string GetEndpointPath(HttpContext context)
    {
        // Intentar obtener el route pattern si está disponible
        var endpoint = context.GetEndpoint();
        if (endpoint is RouteEndpoint routeEndpoint)
        {
            return routeEndpoint.RoutePattern.RawText ?? context.Request.Path;
        }

        // Sanitizar el path para evitar cardinalidad alta
        var path = context.Request.Path.Value ?? "/";

        // Para paths con IDs o GUIDs, normalizarlos
        path = SanitizePath(path);

        return path;
    }

    private static string SanitizePath(string path)
    {
        // Reemplazar UUIDs con {id}
        path = System.Text.RegularExpressions.Regex.Replace(
            path,
            @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
            "{id}");

        // Reemplazar números que parecen IDs con {id}
        path = System.Text.RegularExpressions.Regex.Replace(
            path,
            @"/\d+(/|$)",
            "/{id}$1");

        return path;
    }
}

