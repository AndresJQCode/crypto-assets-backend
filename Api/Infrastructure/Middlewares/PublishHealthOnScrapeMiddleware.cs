using Api.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Infrastructure.Middlewares;

/// <summary>
/// Publica el estado de los health checks en las métricas de Prometheus solo cuando
/// alguien hace GET /metrics (scrape). Así no se ejecutan health checks en segundo plano.
/// </summary>
internal sealed class PublishHealthOnScrapeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == HttpMethods.Get &&
            context.Request.Path.StartsWithSegments("/metrics", StringComparison.OrdinalIgnoreCase))
        {
            await using (var scope = context.RequestServices.CreateAsyncScope())
            {
                var healthCheckService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();
                var report = await healthCheckService.CheckHealthAsync();
                PrometheusExtensions.PublishHealthCheckMetrics(report);
            }
        }

        await next(context);
    }
}
