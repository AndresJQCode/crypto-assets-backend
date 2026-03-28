using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Api.Middlewares;

/// <summary>
/// Middleware to resolve and validate tenant context for API requests
/// </summary>
public class TenantContextMiddleware(RequestDelegate next, ILogger<TenantContextMiddleware> logger)
{
    private static readonly string[] ExcludedPaths = ["/admin", "/auth", "/health", "/metrics", "/swagger", "/scalar"];

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Skip tenant resolution for excluded paths
        if (ShouldSkipTenantResolution(path))
        {
            await next(context);
            return;
        }

        // For /api/* routes, resolve and validate tenant
        if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            var tenantId = ResolveTenantId(context);

            if (tenantId == null)
            {
                logger.LogWarning(
                    "Tenant context required for path {Path} but not found in user claims",
                    path);

                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "TenantContextRequired",
                    message = "Tenant context is required for this endpoint"
                });
                return;
            }

            // Store TenantId in HttpContext.Items for use in repositories and services
            context.Items["TenantId"] = tenantId.Value;

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    "Resolved tenant context: {TenantId} for path {Path}",
                    tenantId,
                    path);
            }
        }

        await next(context);
    }

    private static bool ShouldSkipTenantResolution(string path)
    {
        return ExcludedPaths.Any(excluded =>
            path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase));
    }

    private Guid? ResolveTenantId(HttpContext context)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        // Try to get TenantId from claims
        var tenantIdClaim = user.FindFirst("TenantId")?.Value
                          ?? user.FindFirst(ClaimTypes.GroupSid)?.Value;

        if (string.IsNullOrEmpty(tenantIdClaim))
        {
            return null;
        }

        if (Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            return tenantId;
        }

        logger.LogWarning(
            "Invalid TenantId format in claims: {TenantIdClaim}",
            tenantIdClaim);

        return null;
    }
}

/// <summary>
/// Extension method to register TenantContextMiddleware
/// </summary>
public static class TenantContextMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantContextMiddleware>();
    }
}
