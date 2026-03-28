using System.Threading.RateLimiting;
using Api.Constants;
using Infrastructure;
using Microsoft.Extensions.Options;
using static Infrastructure.AppSettings;

namespace Api.Extensions;

/// <summary>
/// Extensiones para configurar Rate Limiting
/// </summary>
internal static class RateLimitingExtensions
{
    // Excluir endpoints de consulta/gestión de usuario autenticado del rate limit estricto
    // Estos endpoints requieren autenticación pero no son críticos de seguridad
    private static readonly string[] NonStrictAuthEndpoints = { RouteConstants.Auth.Me, RouteConstants.Auth.Logout };

    /// <summary>
    /// Agregar servicios de Rate Limiting
    /// </summary>
    public static IServiceCollection AddRateLimitingServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar RateLimitingSettings desde la configuración
        services.Configure<RateLimitingSettings>(configuration.GetSection("RateLimiting"));

        // Obtener configuración temporal para verificar si está habilitado
        var rateLimitingSettings = configuration.GetSection("RateLimiting").Get<RateLimitingSettings>()
            ?? new RateLimitingSettings();

        if (!rateLimitingSettings.Enabled)
        {
            return services;
        }

        services.AddRateLimiter(options =>
        {
            // Política global por IP
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var path = context.Request.Path.Value ?? string.Empty;

                // Excluir paths configurados
                if (rateLimitingSettings.ShouldExcludePath(path))
                {
                    return RateLimitPartition.GetNoLimiter("excluded");
                }

                // Excluir endpoints de consulta/gestión de usuario autenticado del rate limit estricto
                // Estos endpoints requieren autenticación pero no son críticos de seguridad
                if (NonStrictAuthEndpoints.Any(endpoint =>
                    path.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase)))
                {
                    // Usar rate limit global para estos endpoints
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetPartitionKey(context, "global"),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimitingSettings.PermitLimit,
                            Window = TimeSpan.FromSeconds(rateLimitingSettings.WindowSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0  // Rechazar inmediatamente sin cola
                        });
                }

                // Política más estricta para endpoints críticos de autenticación
                if (RateLimitingSettings.IsAuthEndpoint(path))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetPartitionKey(context, "auth"),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimitingSettings.AuthPermitLimit,
                            Window = TimeSpan.FromSeconds(rateLimitingSettings.AuthWindowSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0  // Rechazar inmediatamente sin cola
                        });
                }

                // Política global por IP
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKey(context, "global"),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitingSettings.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitingSettings.WindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0  // Rechazar inmediatamente sin cola
                    });
            });

            // Política específica para endpoints de autenticación
            options.AddPolicy("AuthPolicy", context =>
            {
                var path = context.Request.Path.Value ?? string.Empty;
                if (!RateLimitingSettings.IsAuthEndpoint(path))
                {
                    return RateLimitPartition.GetNoLimiter("not-auth");
                }

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKey(context, "auth"),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitingSettings.AuthPermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitingSettings.AuthWindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0  // Rechazar inmediatamente sin cola
                    });
            });

            // Política para endpoints generales
            options.AddPolicy("GeneralPolicy", context =>
            {
                var path = context.Request.Path.Value ?? string.Empty;

                if (rateLimitingSettings.ShouldExcludePath(path))
                {
                    return RateLimitPartition.GetNoLimiter("excluded");
                }

                if (RateLimitingSettings.IsAuthEndpoint(path))
                {
                    return RateLimitPartition.GetNoLimiter("auth-handled-by-global");
                }

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKey(context, "general"),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitingSettings.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitingSettings.WindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0  // Rechazar inmediatamente sin cola
                    });
            });

            // Configurar respuesta cuando se excede el límite
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = HeaderConstants.ContentType.ApplicationJson;

                // Determinar el tiempo de retry basado en el tipo de endpoint
                var path = context.HttpContext.Request.Path.Value ?? string.Empty;
                var retryAfter = RateLimitingSettings.IsAuthEndpoint(path)
                    ? rateLimitingSettings.AuthWindowSeconds
                    : rateLimitingSettings.WindowSeconds;

                context.HttpContext.Response.Headers[HeaderConstants.RateLimiting.RetryAfter] = retryAfter.ToString(System.Globalization.CultureInfo.InvariantCulture);

                var response = new
                {
                    error = "Too Many Requests",
                    message = "Has excedido el límite de peticiones. Por favor, intenta de nuevo más tarde.",
                    retryAfter = retryAfter
                };

                await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            };
        });

        return services;
    }

    /// <summary>
    /// Usar Rate Limiting en el pipeline
    /// </summary>
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        var rateLimitingSettings = app.ApplicationServices
            .GetRequiredService<IOptionsMonitor<AppSettings>>()
            .CurrentValue.RateLimiting;

        if (!rateLimitingSettings.Enabled)
        {
            return app;
        }

        return app.UseRateLimiter();
    }

    /// <summary>
    /// Obtener la clave de partición para el rate limiter basada en IP y usuario autenticado
    /// </summary>
    private static string GetPartitionKey(HttpContext context, string policyType)
    {
        // Si el usuario está autenticado, usar su ID como clave
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"{policyType}:user:{userId}";
        }

        // Si no está autenticado, usar la IP
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"{policyType}:ip:{ipAddress}";
    }
}

