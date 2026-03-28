using Prometheus;

namespace Api.Infrastructure.Metrics;

/// <summary>
/// Métricas de Prometheus específicas del proyecto Api (capa de presentación)
/// Para métricas compartidas (caché, DB, permisos), ver Infrastructure.Metrics.InfrastructureMetrics
/// </summary>
internal static class ApiMetrics
{
    private static string _metricPrefix = "";

    /// <summary>
    /// Inicializa las métricas con el nombre de aplicación configurado
    /// </summary>
    public static void Initialize(string applicationName)
    {
        // Prometheus metric names are lowercase by convention (suppress CA1308)
#pragma warning disable CA1308
        _metricPrefix = applicationName.ToLowerInvariant().Replace("-", "_", StringComparison.Ordinal).Replace(" ", "_", StringComparison.Ordinal);
#pragma warning restore CA1308

        // ===== MÉTRICAS HTTP =====

        HttpRequestsTotal = Prometheus.Metrics
            .CreateCounter(
                $"{_metricPrefix}_http_requests_total",
                "Total de requests HTTP recibidos",
                new CounterConfiguration
                {
                    LabelNames = new[] { "method", "endpoint", "status_code" }
                });

        HttpRequestDuration = Prometheus.Metrics
            .CreateHistogram(
                $"{_metricPrefix}_http_request_duration_seconds",
                "Duración de requests HTTP en segundos",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "method", "endpoint" },
                    Buckets = Histogram.ExponentialBuckets(0.001, 2, 15) // 1ms a ~16s
                });

        HttpRequestsInProgress = Prometheus.Metrics
            .CreateGauge(
                $"{_metricPrefix}_http_requests_in_progress",
                "Número de requests HTTP en progreso",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "method" }
                });

        // ===== MÉTRICAS DE MEDIATR (Commands/Queries) =====

        MediatRRequestsTotal = Prometheus.Metrics
            .CreateCounter(
                $"{_metricPrefix}_mediatr_requests_total",
                "Total de comandos/queries de MediatR ejecutados",
                new CounterConfiguration
                {
                    LabelNames = new[] { "request_type", "request_name", "status" }
                });

        MediatRRequestDuration = Prometheus.Metrics
            .CreateHistogram(
                $"{_metricPrefix}_mediatr_request_duration_seconds",
                "Duración de comandos/queries de MediatR en segundos",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "request_type", "request_name" },
                    Buckets = Histogram.ExponentialBuckets(0.001, 2, 15)
                });

        MediatRRequestErrors = Prometheus.Metrics
            .CreateCounter(
                $"{_metricPrefix}_mediatr_request_errors_total",
                "Total de errores en comandos/queries de MediatR",
                new CounterConfiguration
                {
                    LabelNames = new[] { "request_type", "request_name", "error_type" }
                });

        // ===== MÉTRICAS DE HEALTH CHECKS =====
        // Nota: Métricas de Caché, Permisos, Base de Datos y Autenticación
        // están en Infrastructure.Metrics.InfrastructureMetrics

        HealthCheckStatus = Prometheus.Metrics
            .CreateGauge(
                $"{_metricPrefix}_healthcheck_status",
                "Estado del health check (0=unhealthy, 0.5=degraded, 1=healthy)",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "check_name" }
                });

        HealthCheckDuration = Prometheus.Metrics
            .CreateHistogram(
                $"{_metricPrefix}_healthcheck_duration_seconds",
                "Duración de health checks en segundos",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "check_name" },
                    Buckets = Histogram.LinearBuckets(0.01, 0.01, 10) // 10ms a 100ms
                });

        // ===== MÉTRICAS DE APLICACIÓN =====

        UnhandledExceptionsTotal = Prometheus.Metrics
            .CreateCounter(
                $"{_metricPrefix}_unhandled_exceptions_total",
                "Total de excepciones no manejadas",
                new CounterConfiguration
                {
                    LabelNames = new[] { "exception_type", "endpoint" }
                });

        ApplicationUptime = Prometheus.Metrics
            .CreateGauge(
                $"{_metricPrefix}_application_uptime_seconds",
                "Tiempo desde el inicio de la aplicación en segundos");

        ApplicationInfo = Prometheus.Metrics
            .CreateGauge(
                $"{_metricPrefix}_application_info",
                "Información de la aplicación",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "version", "environment" }
                });

        // ===== MÉTRICAS DE CIRCUIT BREAKER =====

        CircuitBreakerState = Prometheus.Metrics
            .CreateGauge(
                $"{_metricPrefix}_circuit_breaker_state",
                "Estado del circuit breaker (0=closed, 0.5=half-open, 1=open)",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "breaker_name" }
                });

        CircuitBreakerOpensTotal = Prometheus.Metrics
            .CreateCounter(
                $"{_metricPrefix}_circuit_breaker_opens_total",
                "Total de veces que se abrió el circuit breaker",
                new CounterConfiguration
                {
                    LabelNames = new[] { "breaker_name" }
                });
    }

    // ===== MÉTRICAS HTTP =====

    /// <summary>
    /// Total de requests HTTP recibidos
    /// </summary>
    public static Counter HttpRequestsTotal { get; private set; } = null!;

    /// <summary>
    /// Duración de requests HTTP en segundos
    /// </summary>
    public static Histogram HttpRequestDuration { get; private set; } = null!;

    /// <summary>
    /// Requests HTTP actualmente en progreso
    /// </summary>
    public static Gauge HttpRequestsInProgress { get; private set; } = null!;

    // ===== MÉTRICAS DE MEDIATR (Commands/Queries) =====

    /// <summary>
    /// Total de comandos/queries ejecutados
    /// </summary>
    public static Counter MediatRRequestsTotal { get; private set; } = null!;

    /// <summary>
    /// Duración de comandos/queries en segundos
    /// </summary>
    public static Histogram MediatRRequestDuration { get; private set; } = null!;

    /// <summary>
    /// Errores en comandos/queries
    /// </summary>
    public static Counter MediatRRequestErrors { get; private set; } = null!;

    // ===== MÉTRICAS DE HEALTH CHECKS =====

    /// <summary>
    /// Estado de health checks (0=unhealthy, 0.5=degraded, 1=healthy)
    /// </summary>
    public static Gauge HealthCheckStatus { get; private set; } = null!;

    /// <summary>
    /// Duración de health checks en segundos
    /// </summary>
    public static Histogram HealthCheckDuration { get; private set; } = null!;

    // ===== MÉTRICAS DE APLICACIÓN =====

    /// <summary>
    /// Total de excepciones no manejadas
    /// </summary>
    public static Counter UnhandledExceptionsTotal { get; private set; } = null!;

    /// <summary>
    /// Tiempo desde el inicio de la aplicación en segundos
    /// </summary>
    public static Gauge ApplicationUptime { get; private set; } = null!;

    /// <summary>
    /// Versión de la aplicación
    /// </summary>
    public static Gauge ApplicationInfo { get; private set; } = null!;

    // ===== MÉTRICAS DE CIRCUIT BREAKER =====

    /// <summary>
    /// Estado del circuit breaker (0=closed, 0.5=half-open, 1=open)
    /// </summary>
    public static Gauge CircuitBreakerState { get; private set; } = null!;

    /// <summary>
    /// Total de aperturas del circuit breaker
    /// </summary>
    public static Counter CircuitBreakerOpensTotal { get; private set; } = null!;
}
