using Prometheus;

namespace Infrastructure.Metrics;

/// <summary>
/// Métricas de Prometheus para el proyecto Infrastructure
/// </summary>
public static class InfrastructureMetrics
{
    private static string _metricPrefix = "template";

    /// <summary>
    /// Inicializa las métricas con el nombre de aplicación configurado
    /// </summary>
    public static void Initialize(string applicationName)
    {
        // Prometheus metric names are lowercase by convention (suppress CA1308)
#pragma warning disable CA1308
        _metricPrefix = applicationName.ToLowerInvariant().Replace("-", "_", StringComparison.Ordinal).Replace(" ", "_", StringComparison.Ordinal);
#pragma warning restore CA1308

        // ===== MÉTRICAS DE CACHÉ GENÉRICO =====

        CacheHitsTotal = Prometheus.Metrics
            .CreateCounter(
                $"{_metricPrefix}_cache_hits_total",
                "Total de objetos encontrados en caché (cache hits)",
                new CounterConfiguration
                {
                    LabelNames = new[] { "cache_type", "object_type" }
                });

        CacheMissesTotal = Prometheus.Metrics
            .CreateCounter(
                $"{_metricPrefix}_cache_misses_total",
                "Total de objetos NO encontrados en caché (cache misses)",
                new CounterConfiguration
                {
                    LabelNames = new[] { "cache_type", "object_type" }
                });

        CacheOperationDuration = Prometheus.Metrics
            .CreateHistogram(
                $"{_metricPrefix}_cache_operation_duration_seconds",
                "Duración de operaciones de caché en segundos",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "operation", "cache_type" },
                    Buckets = Histogram.ExponentialBuckets(0.0001, 2, 12)
                });

        // ===== MÉTRICAS DE CACHÉ DE PERMISOS (específico) =====

        PermissionCacheHitsTotal = Prometheus.Metrics
            .CreateCounter(
                $"{_metricPrefix}_permission_cache_hits_total",
                "Total de permisos encontrados en caché (cache hits)",
                new CounterConfiguration
                {
                    LabelNames = new[] { "cache_name", "key_type" }
                });

        PermissionCacheMissesTotal = Prometheus.Metrics
            .CreateCounter(
                $"{_metricPrefix}_permission_cache_misses_total",
                "Total de permisos NO encontrados en caché (cache misses)",
                new CounterConfiguration
                {
                    LabelNames = new[] { "cache_name", "key_type" }
                });

        PermissionCacheOperationDuration = Prometheus.Metrics
            .CreateHistogram(
                $"{_metricPrefix}_permission_cache_operation_duration_seconds",
                "Duración de operaciones en el caché de permisos en segundos",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "operation", "cache_name" },
                    Buckets = Histogram.ExponentialBuckets(0.0001, 2, 12)
                });

        // ===== MÉTRICAS DE PERMISOS =====

        PermissionChecksTotal = Prometheus.Metrics
            .CreateCounter(
                $"{_metricPrefix}_permission_checks_total",
                "Total de verificaciones de permisos",
                new CounterConfiguration
                {
                    LabelNames = new[] { "permission_name", "result" }
                });

        PermissionCheckDuration = Prometheus.Metrics
            .CreateHistogram(
                $"{_metricPrefix}_permission_check_duration_seconds",
                "Duración de verificaciones de permisos en segundos",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "permission_name", "from_cache" },
                    Buckets = Histogram.ExponentialBuckets(0.0001, 2, 12)
                });

        // ===== MÉTRICAS DE BASE DE DATOS =====

        DatabaseQueryDuration = Prometheus.Metrics
            .CreateHistogram(
                $"{_metricPrefix}_database_query_duration_seconds",
                "Duración de queries de base de datos en segundos",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "query_type", "entity" },
                    Buckets = Histogram.ExponentialBuckets(0.001, 2, 15)
                });

        DatabaseQueriesTotal = Prometheus.Metrics
            .CreateCounter(
                $"{_metricPrefix}_database_queries_total",
                "Total de queries de base de datos ejecutadas",
                new CounterConfiguration
                {
                    LabelNames = new[] { "query_type", "entity", "status" }
                });

        DatabaseErrorsTotal = Prometheus.Metrics
            .CreateCounter(
                $"{_metricPrefix}_database_errors_total",
                "Total de errores de base de datos",
                new CounterConfiguration
                {
                    LabelNames = new[] { "error_type", "entity" }
                });

        // ===== MÉTRICAS DE AUTENTICACIÓN =====

        AuthenticationAttemptsTotal = Prometheus.Metrics
            .CreateCounter(
                $"{_metricPrefix}_authentication_attempts_total",
                "Total de intentos de autenticación",
                new CounterConfiguration
                {
                    LabelNames = new[] { "method", "result" }
                });

        JwtTokensGeneratedTotal = Prometheus.Metrics
            .CreateCounter(
                $"{_metricPrefix}_jwt_tokens_generated_total",
                "Total de tokens JWT generados",
                new CounterConfiguration
                {
                    LabelNames = new[] { "token_type" }
                });

        UserOperationsTotal = Prometheus.Metrics
            .CreateCounter(
                $"{_metricPrefix}_user_operations_total",
                "Total de operaciones de usuarios",
                new CounterConfiguration
                {
                    LabelNames = new[] { "operation", "status" }
                });
    }

    // ===== MÉTRICAS DE CACHÉ GENÉRICO =====

    /// <summary>
    /// Contador de objetos encontrados en caché genérico (cache hit)
    /// Labels: cache_type (ej: "memory", "redis"), object_type (ej: nombre del tipo de objeto)
    /// </summary>
    public static Counter CacheHitsTotal { get; private set; } = null!;

    /// <summary>
    /// Contador de objetos NO encontrados en caché genérico (cache miss)
    /// Labels: cache_type (ej: "memory", "redis"), object_type (ej: nombre del tipo de objeto)
    /// </summary>
    public static Counter CacheMissesTotal { get; private set; } = null!;

    /// <summary>
    /// Duración de operaciones en caché genérico
    /// Labels: operation (ej: "get", "set", "remove"), cache_type (ej: "memory", "redis")
    /// </summary>
    public static Histogram CacheOperationDuration { get; private set; } = null!;

    // ===== MÉTRICAS DE CACHÉ DE PERMISOS (específico) =====

    /// <summary>
    /// Contador de permisos encontrados en caché (cache hit)
    /// Labels: cache_name (ej: "permission_cache"), key_type (ej: "user_permissions")
    /// </summary>
    public static Counter PermissionCacheHitsTotal { get; private set; } = null!;

    /// <summary>
    /// Contador de permisos NO encontrados en caché (cache miss)
    /// Labels: cache_name (ej: "permission_cache"), key_type (ej: "user_permissions")
    /// </summary>
    public static Counter PermissionCacheMissesTotal { get; private set; } = null!;

    /// <summary>
    /// Duración de operaciones en el caché de permisos
    /// Labels: operation (ej: "get", "set", "remove"), cache_name (ej: "permission_cache")
    /// </summary>
    public static Histogram PermissionCacheOperationDuration { get; private set; } = null!;

    // ===== MÉTRICAS DE PERMISOS =====

    /// <summary>
    /// Total de verificaciones de permisos
    /// Labels: permission_name, result
    /// </summary>
    public static Counter PermissionChecksTotal { get; private set; } = null!;

    /// <summary>
    /// Duración de verificaciones de permisos en segundos
    /// Labels: permission_name, from_cache
    /// </summary>
    public static Histogram PermissionCheckDuration { get; private set; } = null!;

    // ===== MÉTRICAS DE BASE DE DATOS =====

    /// <summary>
    /// Duración de queries de base de datos en segundos
    /// Labels: query_type, entity
    /// </summary>
    public static Histogram DatabaseQueryDuration { get; private set; } = null!;

    /// <summary>
    /// Total de queries de base de datos ejecutadas
    /// Labels: query_type, entity, status
    /// </summary>
    public static Counter DatabaseQueriesTotal { get; private set; } = null!;

    /// <summary>
    /// Total de errores de base de datos
    /// Labels: error_type, entity
    /// </summary>
    public static Counter DatabaseErrorsTotal { get; private set; } = null!;

    // ===== MÉTRICAS DE AUTENTICACIÓN =====

    /// <summary>
    /// Total de intentos de autenticación
    /// Labels: method, result
    /// </summary>
    public static Counter AuthenticationAttemptsTotal { get; private set; } = null!;

    /// <summary>
    /// Total de tokens JWT generados
    /// Labels: token_type
    /// </summary>
    public static Counter JwtTokensGeneratedTotal { get; private set; } = null!;

    /// <summary>
    /// Total de operaciones de usuarios
    /// Labels: operation, status
    /// </summary>
    public static Counter UserOperationsTotal { get; private set; } = null!;
}
