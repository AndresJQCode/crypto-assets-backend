using Api.Infrastructure.Metrics;
using Api.Infrastructure.Middlewares;
using Infrastructure;
using Infrastructure.Metrics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Prometheus;

namespace Api.Extensions;

internal static class PrometheusExtensions
{
    /// <summary>
    /// Agrega los servicios necesarios para Prometheus
    /// </summary>
    public static IServiceCollection AddPrometheusMetrics(this IServiceCollection services, IConfiguration configuration)
    {
        // Obtener el nombre de la aplicación desde la configuración
        var appSettings = configuration.Get<AppSettings>();
        var applicationName = appSettings?.ApplicationName ?? "template";

        // Inicializar las métricas con el nombre de la aplicación
        ApiMetrics.Initialize(applicationName);
        InfrastructureMetrics.Initialize(applicationName);

        return services;
    }

    /// <summary>
    /// Configura el middleware de Prometheus y los endpoints de métricas
    /// </summary>
    public static IApplicationBuilder UsePrometheusMetrics(this IApplicationBuilder app, IWebHostEnvironment environment)
    {
        // Publicar health checks en métricas solo cuando se scrapea /metrics (sin tarea en segundo plano)
        app.UseMiddleware<PublishHealthOnScrapeMiddleware>();

        // Middleware de captura de métricas HTTP (debe ir antes de otros middlewares)
        app.UseMiddleware<PrometheusMetricsMiddleware>();

        // Inicializar métrica de información de aplicación
        var version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown";
        var environmentName = environment.EnvironmentName;
        ApiMetrics.ApplicationInfo.WithLabels(version, environmentName).Set(1);

        // Task para actualizar métricas periódicamente
        _ = Task.Run(async () =>
        {
            var startTime = DateTime.UtcNow;
            while (true)
            {
                try
                {
                    // Actualizar uptime
                    var uptime = (DateTime.UtcNow - startTime).TotalSeconds;
                    ApiMetrics.ApplicationUptime.Set(uptime);

                    await Task.Delay(TimeSpan.FromSeconds(15));
                }
                catch
                {
                    // Ignorar errores en el background task
                }
            }
        });

        return app;
    }

    /// <summary>
    /// Mapea los endpoints de métricas de Prometheus
    /// </summary>
    public static IEndpointRouteBuilder MapPrometheusMetrics(this IEndpointRouteBuilder endpoints)
    {
        // Endpoint principal de métricas (formato Prometheus)
        endpoints.MapMetrics("/metrics")
            .WithTags("Metrics")
            .WithSummary("Métricas de Prometheus")
            .WithDescription("Endpoint de métricas en formato Prometheus para scraping");

        // Endpoint de métricas en texto plano (para debugging)
        endpoints.MapGet("/metrics-text", async context =>
        {
            var appSettings = context.RequestServices.GetRequiredService<IOptionsMonitor<AppSettings>>();
            var applicationName = appSettings.CurrentValue.ApplicationName;

            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync($"=== {applicationName} Metrics ===\n\n");
            await context.Response.WriteAsync("Para ver las métricas en formato Prometheus, visita: /metrics\n\n");
            await context.Response.WriteAsync($"Tiempo de ejecución: {ApiMetrics.ApplicationUptime.Value:F0} segundos\n");
            await context.Response.WriteAsync($"Version: {typeof(Program).Assembly.GetName().Version}\n");
            await context.Response.WriteAsync($"Environment: {context.RequestServices.GetRequiredService<IWebHostEnvironment>().EnvironmentName}\n");
        })
        .WithTags("Metrics")
        .WithSummary("Información de métricas")
        .WithDescription("Información básica sobre las métricas disponibles");

        return endpoints;
    }

    /// <summary>
    /// Agrega soporte para publicar métricas de health checks en Prometheus
    /// </summary>
    public static void PublishHealthCheckMetrics(HealthReport report)
    {
        foreach (var entry in report.Entries)
        {
            var checkName = entry.Key;
            var status = entry.Value.Status;
            var duration = entry.Value.Duration.TotalSeconds;

            // Convertir estado a valor numérico
            double statusValue = status switch
            {
                HealthStatus.Healthy => 1.0,
                HealthStatus.Degraded => 0.5,
                HealthStatus.Unhealthy => 0.0,
                _ => 0.0
            };

            // Publicar métricas
            ApiMetrics.HealthCheckStatus
                .WithLabels(checkName)
                .Set(statusValue);

            ApiMetrics.HealthCheckDuration
                .WithLabels(checkName)
                .Observe(duration);
        }
    }
}

