using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using Api.Application.Dtos;
using Api.Application.Queries.PermissionQueries;
using Api.Constants;
using Api.Infrastructure.Services;
using Infrastructure;
using Infrastructure.Constants;
using Infrastructure.Metrics;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Api.Infrastructure.Middlewares;

internal sealed class PermissionAuthorizationMiddleware(
    RequestDelegate next,
    IPermissionCircuitBreakerService circuitBreakerService,
    IOptionsMonitor<AppSettings> appSettings,
    ILogger<PermissionAuthorizationMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false // Optimización: no indentar en producción
    };

    private static async Task WriteJsonErrorResponse(HttpContext context, int statusCode, string message, string? details = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = HeaderConstants.ContentType.ApplicationJson;

        var traceId = context.TraceIdentifier;
        var errorResponse = new ErrorResponseDto(message, statusCode, details, traceId);

        var jsonResponse = JsonSerializer.Serialize(errorResponse, JsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("PermissionAuthorizationMiddleware: Iniciando para path: {Path}, Method: {Method}",
                context.Request.Path, context.Request.Method);
        }

        var middlewareOptions = appSettings.CurrentValue.PermissionMiddleware;

        // Verificar si el path debe ser excluido
        if (middlewareOptions.ShouldExcludePath(context.Request.Path))
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("PermissionAuthorizationMiddleware: Path {Path} está excluido, saltando verificación", context.Request.Path);
            }

            await next(context);
            return;
        }

        var endpoint = context.GetEndpoint();
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("PermissionAuthorizationMiddleware: Endpoint resuelto: {EndpointDisplayName}, Endpoint es null: {IsNull}",
                endpoint?.DisplayName, endpoint == null);
        }

        if (endpoint == null)
        {
            logger.LogWarning("PermissionAuthorizationMiddleware: Endpoint es NULL para path {Path}. Esto puede indicar un problema con el routing.",
                context.Request.Path);
        }

        var permissionAttribute = endpoint?.Metadata.GetMetadata<RequirePermissionAttribute>();

        if (permissionAttribute == null)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("PermissionAuthorizationMiddleware: Endpoint {EndpointDisplayName} no tiene RequirePermissionAttribute, saltando verificación. Metadata disponible: {MetadataCount}",
                    endpoint?.DisplayName, endpoint?.Metadata?.Count ?? 0);
            }

            await next(context);
            return;
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("PermissionAuthorizationMiddleware: Endpoint requiere permiso - Resource: {Resource}, Action: {Action}",
                permissionAttribute.Resource, permissionAttribute.Action);
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Validación de autenticación
            var authResult = await ValidateAuthenticationAsync(context);
            if (!authResult.IsValid)
            {
                await WriteJsonErrorResponse(context, authResult.StatusCode, authResult.Message, authResult.Details);
                return;
            }

            // Verificación de permisos con circuit breaker
            var permissionResult = await CheckPermissionAsync(context, permissionAttribute, authResult.UserId!.Value);
            if (!permissionResult.IsValid)
            {
                await WriteJsonErrorResponse(context, permissionResult.StatusCode, permissionResult.Message, permissionResult.Details);
                return;
            }

            // Log de acceso exitoso para auditoría
            if (middlewareOptions.EnableAuditLogging && logger.IsEnabled(LogLevel.Information))
            {
                var requestDetails = middlewareOptions.IncludeRequestDetails
                    ? $" desde {context.Connection.RemoteIpAddress}"
                    : string.Empty;

                logger.LogInformation("Acceso autorizado: Usuario {UserId} accedió a {Resource}.{Action} en {Endpoint}{RequestDetails}",
                    authResult.UserId, permissionAttribute.Resource, permissionAttribute.Action, context.Request.Path, requestDetails);
            }

            // Registrar métricas de Prometheus
            if (middlewareOptions.EnablePerformanceMetrics)
            {
                stopwatch.Stop();

                var permissionKey = $"{permissionAttribute.Resource}.{permissionAttribute.Action}";

                // Métrica: verificación de permiso exitosa
                InfrastructureMetrics.PermissionChecksTotal
                    .WithLabels(permissionKey, "allowed")
                    .Inc();

                InfrastructureMetrics.PermissionCheckDuration
                    .WithLabels(permissionKey, "middleware")
                    .Observe(stopwatch.Elapsed.TotalSeconds);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado en middleware de permisos para {Path}", context.Request.Path);
            await WriteJsonErrorResponse(context, 500, "Error interno del servidor", "Ocurrió un error inesperado");
        }
        finally
        {
            if (middlewareOptions.EnablePerformanceMetrics)
            {
                stopwatch.Stop();
            }
        }

        await next(context);
    }

    private Task<AuthValidationResult> ValidateAuthenticationAsync(HttpContext context)
    {
        var user = context.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            logger.LogWarning("Intento de acceso no autenticado a {Path}", context.Request.Path);

            // Métrica de Prometheus: intento de acceso no autenticado
            InfrastructureMetrics.AuthenticationAttemptsTotal
                .WithLabels(MetricsLabelsConstants.Middleware.Label, MetricsLabelsConstants.Middleware.NotAuthenticated)
                .Inc();

            return Task.FromResult(new AuthValidationResult(false, 401, "No autorizado", "El usuario no está autenticado"));
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            logger.LogWarning("Claim de usuario inválido para {Path}: {UserIdClaim}", context.Request.Path, userIdClaim);

            // Métrica de Prometheus: claim inválido
            InfrastructureMetrics.AuthenticationAttemptsTotal
                .WithLabels(MetricsLabelsConstants.Middleware.Label, MetricsLabelsConstants.Middleware.InvalidClaim)
                .Inc();

            return Task.FromResult(new AuthValidationResult(false, 401, "ID de usuario no válido", "El claim de ID de usuario no es un GUID válido"));
        }

        return Task.FromResult(new AuthValidationResult(true, 0, string.Empty, string.Empty, userId));
    }

    private async Task<PermissionValidationResult> CheckPermissionAsync(HttpContext context, RequirePermissionAttribute permissionAttribute, Guid userId)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("CheckPermissionAsync: Verificando permiso para usuario {UserId}, recurso: {Resource}, acción: {Action}",
                    userId, permissionAttribute.Resource, permissionAttribute.Action);
            }

            var hasPermission = await circuitBreakerService.ExecuteWithCircuitBreakerAsync(
                async () =>
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("------CheckPermissionAsync: Enviando CheckUserPermissionQuery a MediatR");
                    }

                    var query = new CheckUserPermissionQuery(
                        userId,
                        permissionAttribute.Resource,
                        permissionAttribute.Action);

                    // Usar HttpContext.RequestServices para obtener MediatR dentro del scope del request
                    // Esto asegura que los behaviors (como TransactionBehavior) puedan resolver servicios scoped como ApiContext
                    var mediator = context.RequestServices.GetRequiredService<IMediator>();
                    var result = await mediator.Send(query);
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("CheckPermissionAsync: MediatR retornó resultado: {Result}", result);
                    }

                    return result;
                },
                fallbackValue: false
            );

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("CheckPermissionAsync: Resultado final de verificación de permiso: {HasPermission}", hasPermission);
            }

            stopwatch.Stop();

            var permissionKey = $"{permissionAttribute.Resource}.{permissionAttribute.Action}";

            // Registrar métricas de Prometheus
            if (appSettings.CurrentValue.PermissionMiddleware.EnablePerformanceMetrics)
            {
                InfrastructureMetrics.PermissionChecksTotal
                    .WithLabels(permissionKey, hasPermission ? "allowed" : "denied")
                    .Inc();

                InfrastructureMetrics.PermissionCheckDuration
                    .WithLabels(permissionKey, "middleware")
                    .Observe(stopwatch.Elapsed.TotalSeconds);
            }

            if (hasPermission)
            {
                return new PermissionValidationResult(true, 0, string.Empty, string.Empty);
            }

            // Determinar el tipo de error basado en el estado del circuit breaker
            var circuitState = circuitBreakerService.CircuitState;
            var isCircuitOpen = circuitBreakerService.IsCircuitOpen;

            if (isCircuitOpen)
            {
                logger.LogWarning("Circuit breaker abierto durante verificación de permisos para usuario {UserId}", userId);

                // Métricas de Prometheus para circuit breaker
                double stateValue = circuitState switch
                {
                    "Closed" => 0.0,
                    "HalfOpen" => 0.5,
                    "Open" => 1.0,
                    _ => 0.0
                };

                Api.Infrastructure.Metrics.ApiMetrics.CircuitBreakerState
                    .WithLabels(MetricsLabelsConstants.Middleware.PermissionService)
                    .Set(stateValue);

                if (circuitState == "Open")
                {
                    Api.Infrastructure.Metrics.ApiMetrics.CircuitBreakerOpensTotal
                        .WithLabels(MetricsLabelsConstants.Middleware.PermissionService)
                        .Inc();
                }

                return new PermissionValidationResult(false, 503,
                    "Servicio de permisos temporalmente no disponible",
                    $"El servicio de permisos está temporalmente no disponible (Circuit Breaker: {circuitState}). El usuario {userId} no pudo ser verificado.");
            }

            // Usuario no tiene permisos
            logger.LogWarning("Acceso denegado: Usuario {UserId} no tiene permiso {Resource}.{Action} para {Path}",
                userId, permissionAttribute.Resource, permissionAttribute.Action, context.Request.Path);

            // Métrica adicional de permisos denegados
            InfrastructureMetrics.UserOperationsTotal
                .WithLabels(MetricsLabelsConstants.Middleware.PermissionDenied, MetricsLabelsConstants.Authentication.Failed)
                .Inc();

            return new PermissionValidationResult(false, 403,
                $"No tiene permisos para {permissionAttribute.Resource}.{permissionAttribute.Action}",
                $"El usuario {userId} no tiene el permiso requerido: {permissionAttribute.Resource}.{permissionAttribute.Action}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Error inesperado durante verificación de permisos para usuario {UserId}", userId);
            return new PermissionValidationResult(false, 500,
                "Error interno del servidor",
                "Ocurrió un error inesperado durante la verificación de permisos");
        }
    }
}

/// <summary>
/// Atributo para marcar endpoints que requieren permisos específicos
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
internal sealed class RequirePermissionAttribute : Attribute
{
    public string Resource { get; }
    public string Action { get; }

    public RequirePermissionAttribute(string resource, string action)
    {
        Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }
}

/// <summary>
/// Resultado de validación de autenticación
/// </summary>
internal sealed record AuthValidationResult(bool IsValid, int StatusCode, string Message, string Details, Guid? UserId = null);

/// <summary>
/// Resultado de validación de permisos
/// </summary>
internal sealed record PermissionValidationResult(bool IsValid, int StatusCode, string Message, string Details);
