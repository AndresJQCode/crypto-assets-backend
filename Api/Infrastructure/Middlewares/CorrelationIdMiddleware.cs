using Serilog.Context;

namespace Api.Infrastructure.Middlewares;

/// <summary>
/// Middleware para generar y propagar Correlation ID en todos los logs.
/// Captura el header X-Correlation-ID si existe, o genera uno nuevo.
/// </summary>
internal sealed class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string CorrelationIdItemKey = "CorrelationId";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Obtener o generar Correlation ID
        var correlationId = GetOrGenerateCorrelationId(context);

        // Guardar en HttpContext para acceso durante la request
        context.Items[CorrelationIdItemKey] = correlationId;

        // Agregar al header de respuesta para que el cliente pueda usarlo
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        // Usar Serilog LogContext para propagar el Correlation ID a todos los logs
        // Esto hace que todos los logs generados durante esta request incluyan automáticamente el Correlation ID
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogDebug("Correlation ID establecido: {CorrelationId}", correlationId);

            await _next(context);
        }
    }

    /// <summary>
    /// Obtiene el Correlation ID del header de la request, o genera uno nuevo si no existe.
    /// </summary>
    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        // Intentar obtener el Correlation ID del header de la request
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationIdHeader) &&
            !string.IsNullOrWhiteSpace(correlationIdHeader))
        {
            return correlationIdHeader.ToString();
        }

        // Si no existe, generar uno nuevo (GUID)
        return Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Método de extensión para obtener el Correlation ID del HttpContext.
    /// </summary>
    public static string GetCorrelationId(HttpContext context)
    {
        if (context.Items.TryGetValue(CorrelationIdItemKey, out var correlationId) && correlationId is string id)
        {
            return id;
        }

        // Si no existe, generar uno (no debería pasar si el middleware está configurado correctamente)
        return Guid.NewGuid().ToString();
    }
}
