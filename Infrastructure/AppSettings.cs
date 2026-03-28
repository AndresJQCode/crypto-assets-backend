using System.Diagnostics.CodeAnalysis;

namespace Infrastructure;

[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Nested configuration classes are intentionally public for Options pattern")]
public class AppSettings
{
    public string ApplicationName { get; set; } = "template";
    public string CompanyName { get; set; } = string.Empty;
    public required string AllowedHosts { get; set; }
    public required OpenApiSettings OpenApi { get; set; }
    public required EmailSettings EmailService { get; set; }
    public required VaultSettings Vault { get; set; }
    public required ServiceBusSettings ServiceBus { get; set; }
    public required ConnectionStringsSettings ConnectionStrings { get; set; }
    public required AuthenticationSettings Authentication { get; set; }
    public required JwtConfiguration JwtSettings { get; set; }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI properties should not be strings", Justification = "String is easier to configure in appsettings.json")]
    public required string FrontUrl { get; set; }
    public required InfobipSettings Infobip { get; set; }
    public required PermissionMiddlewareSettings PermissionMiddleware { get; set; }
    public RateLimitingSettings RateLimiting { get; set; } = new();
    public RecaptchaSettings Recaptcha { get; set; } = new();
    public PasswordResetSettings PasswordReset { get; set; } = new();
    public CorsSettings Cors { get; set; } = new();
    public MemoryCacheSettings MemoryCache { get; set; } = new();
    public PaginationSettings Pagination { get; set; } = new();
    public CircuitBreakerSettings CircuitBreaker { get; set; } = new();
    public ConnectorsSettings? Connectors { get; set; }
    public PubSubSettings? PubSub { get; set; }
    public required string EncryptionKey { get; set; }

    public class ConnectorsSettings
    {
        public ShopifyConnectorSettings? Shopify { get; set; }

        public class ShopifyConnectorSettings
        {
            public required string ClientId { get; set; }
            public required string ClientSecret { get; set; }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI properties should not be strings", Justification = "String is easier to configure in appsettings.json")]
            public required string RedirectUri { get; set; }
            public required string[] Scopes { get; set; }
            public required string ApiVersion { get; set; }
            public int TimeoutSeconds { get; set; } = 30;
        }
    }

    public class EmailSettings
    {
        /// <summary>
        /// Email provider to use: "Infobip", "SendGrid", "Smtp"
        /// </summary>
        public string Provider { get; set; } = "Infobip";
        public required string FromEmail { get; set; }
        public required string FromName { get; set; }
        public required string TestEmailTo { get; set; }
        public required string HeaderImage { get; set; }
    }

    public class InfobipSettings
    {
        public required string BasePath { get; set; } = string.Empty;
        public required string ApiKey { get; set; } = string.Empty;
    }


    public class OpenApiSettings
    {
        public required DocumentSettings Document { get; set; }

        public class DocumentSettings
        {
            public required string Description { get; set; }
            public required string Title { get; set; }
            public required string Version { get; set; }
        }
    }

    public class VaultSettings
    {
        public required string Name { get; set; }
        public required string TenantId { get; set; }
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
    }

    public class ServiceBusSettings
    {
        public required string ConnectionStrings { get; set; }
        public required string TopicEBilling { get; set; }
        public required string SuscriptionEBilling { get; set; }
    }

    public class ConnectionStringsSettings
    {
        public required string DB { get; set; }
    }

    public class AuthenticationSettings
    {
        public required MicrosoftOAuthSettings Microsoft { get; set; }
        public required GoogleOAuthSettings Google { get; set; }
    }

    public class MicrosoftOAuthSettings
    {
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI properties should not be strings", Justification = "String is easier to configure in appsettings.json")]
        public required string RedirectUri { get; set; }
        public required string TenantId { get; set; }
        /// <summary>
        /// Timeout en segundos para llamadas HTTP a Microsoft OAuth
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;
    }

    public class GoogleOAuthSettings
    {
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI properties should not be strings", Justification = "String is easier to configure in appsettings.json")]
        public required string RedirectUri { get; set; }
        /// <summary>
        /// Timeout en segundos para llamadas HTTP a Google OAuth
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;
    }

    public class PermissionMiddlewareSettings
    {
        /// <summary>
        /// Habilitar logging detallado de auditoría
        /// </summary>
        public bool EnableAuditLogging { get; set; } = true;

        /// <summary>
        /// Incluir información adicional en los logs de auditoría
        /// </summary>
        public bool IncludeRequestDetails { get; set; } = true;

        /// <summary>
        /// Tiempo de timeout para verificaciones de permisos (en segundos)
        /// </summary>
        public int PermissionTimeoutSeconds { get; set; } = 5;

        /// <summary>
        /// Habilitar métricas de rendimiento
        /// </summary>
        public bool EnablePerformanceMetrics { get; set; }

        /// <summary>
        /// Endpoints que deben ser excluidos de la verificación de permisos
        /// </summary>
        public HashSet<string> ExcludedPaths { get; init; } = new()
        {
            "/health",
            "/metrics",
            "/swagger"
        };

        /// <summary>
        /// Verificar si un path debe ser excluido
        /// </summary>
        public bool ShouldExcludePath(string path)
        {
            return ExcludedPaths.Any(excludedPath =>
                path.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class JwtConfiguration
    {
        public required string SecretKey { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public int ExpirationMinutes { get; set; } = 60;
        public int RefreshTokenExpirationDays { get; set; } = 30;
    }

    public class RateLimitingSettings
    {
        /// <summary>
        /// Habilitar rate limiting global
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Número de peticiones permitidas por ventana de tiempo
        /// </summary>
        public int PermitLimit { get; set; } = 100;

        /// <summary>
        /// Ventana de tiempo en segundos
        /// </summary>
        public int WindowSeconds { get; set; } = 60;

        /// <summary>
        /// Número de peticiones permitidas para endpoints de autenticación
        /// </summary>
        public int AuthPermitLimit { get; set; } = 10;

        /// <summary>
        /// Ventana de tiempo para endpoints de autenticación en segundos
        /// </summary>
        public int AuthWindowSeconds { get; set; } = 60;

        /// <summary>
        /// Endpoints que deben ser excluidos del rate limiting
        /// </summary>
        public HashSet<string> ExcludedPaths { get; init; } = new()
        {
            "/health",
            "/metrics",
            "/swagger",
            "/scalar/v1"
        };

        /// <summary>
        /// Verificar si un path debe ser excluido del rate limiting
        /// </summary>
        public bool ShouldExcludePath(string path)
        {
            return ExcludedPaths.Any(excludedPath =>
                path.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Verificar si un path es un endpoint de autenticación
        /// </summary>
        public static bool IsAuthEndpoint(string path)
        {
            // Solo aplicar rate limit estricto a endpoints críticos de autenticación
            // Excluir endpoints de consulta como /auth/me, /auth/logout, etc.
            var strictAuthEndpoints = new[]
            {
                "/auth/login",
                "/auth/register",
                "/auth/exchangeCode",
                "/auth/refresh",
                "/auth/forgotPassword"
            };

            return strictAuthEndpoints.Any(endpoint =>
                path.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class RecaptchaSettings
    {
        /// <summary>
        /// Habilitar validación de reCAPTCHA
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Google Cloud Project ID (requerido para reCAPTCHA Enterprise).
        /// </summary>
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// Contenido JSON de la cuenta de servicio (opcional). Ideal para Azure DevOps / variables de entorno:
        /// guardar el JSON como secreto e inyectarlo aquí. Tiene prioridad sobre CredentialsPath.
        /// </summary>
        public string? CredentialsJson { get; set; }

        /// <summary>
        /// Site key de reCAPTCHA (para el frontend). En Enterprise coincide con la key del sitio.
        /// </summary>
        public string SiteKey { get; set; } = string.Empty;

        /// <summary>
        /// Acción esperada en el token (debe coincidir con el atributo action del widget en el frontend).
        /// Ej: "login", "register", "submit". Si está vacío no se valida la acción.
        /// </summary>
        public string ExpectedAction { get; set; } = "submit";

        /// <summary>
        /// Score mínimo para reCAPTCHA v3 (0.0 a 1.0)
        /// Recomendado: 0.5 para endpoints sensibles, 0.3 para otros
        /// </summary>
        public double MinimumScore { get; set; } = 0.5;

        /// <summary>
        /// Timeout en segundos para la verificación con Google
        /// </summary>
        public int TimeoutSeconds { get; set; } = 5;

        /// <summary>
        /// Endpoints que requieren validación de reCAPTCHA
        /// </summary>
        public HashSet<string> RequiredEndpoints { get; init; } = new()
        {
            "/auth/login",
            "/auth/register",
            "/auth/forgotPassword"
        };

        /// <summary>
        /// Verificar si un endpoint requiere validación de reCAPTCHA
        /// </summary>
        public bool RequiresValidation(string path)
        {
            return RequiredEndpoints.Any(endpoint =>
                path.Equals(endpoint, StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(endpoint + "/", StringComparison.OrdinalIgnoreCase));
        }
    }

    public class PasswordResetSettings
    {
        /// <summary>
        /// Habilitar la validación de intervalos para el reseteo de contraseña
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Intervalos incrementales en segundos para cada intento de envío.
        /// El índice del array representa el número de intento (0 = primer intento, 1 = segundo intento, etc.)
        /// Si no se especifica, se usan los valores por defecto: [0, 30, 60, 300, 1800, 3600]
        /// </summary>
        public int[] IncrementalIntervalsSeconds { get; set; } = [0, 30, 60, 300, 1800, 3600];

        /// <summary>
        /// Obtener el intervalo en segundos para un intento específico
        /// </summary>
        public int GetIntervalForAttempt(int attemptNumber)
        {
            if (attemptNumber < 0)
                return IncrementalIntervalsSeconds[0];

            if (attemptNumber >= IncrementalIntervalsSeconds.Length)
                return IncrementalIntervalsSeconds[^1]; // Retorna el último intervalo para intentos posteriores

            return IncrementalIntervalsSeconds[attemptNumber];
        }
    }

    public class CorsSettings
    {
        /// <summary>
        /// Orígenes permitidos para CORS. Use "*" solo en desarrollo.
        /// Ejemplo: ["https://miapp.com", "https://admin.miapp.com"]
        /// </summary>
        public string[] AllowedOrigins { get; set; } = [];

        /// <summary>
        /// Métodos HTTP permitidos. Por defecto: GET, POST, PUT, DELETE, PATCH, OPTIONS
        /// </summary>
        public string[] AllowedMethods { get; set; } = ["GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS"];

        /// <summary>
        /// Headers permitidos. Por defecto permite headers comunes.
        /// </summary>
        public string[] AllowedHeaders { get; set; } = ["Content-Type", "Authorization", "X-Requested-With", "X-Correlation-ID"];

        /// <summary>
        /// Headers expuestos al cliente
        /// </summary>
        public string[] ExposedHeaders { get; set; } = ["X-Correlation-ID"];

        /// <summary>
        /// Permitir credenciales (cookies, authorization headers).
        /// No puede usarse junto con AllowedOrigins = ["*"]
        /// </summary>
        public bool AllowCredentials { get; set; } = true;

        /// <summary>
        /// Tiempo en segundos que el navegador puede cachear la respuesta preflight
        /// </summary>
        public int PreflightMaxAgeSeconds { get; set; } = 600;
    }

    public class MemoryCacheSettings
    {
        /// <summary>
        /// Límite de tamaño del caché (número de unidades, no bytes).
        /// Cada entrada debe especificar su tamaño con SetSize().
        /// </summary>
        public long SizeLimit { get; set; } = 10000;

        /// <summary>
        /// Porcentaje de compactación cuando se alcanza el límite (0.0 a 1.0)
        /// </summary>
        public double CompactionPercentage { get; set; } = 0.25;

        /// <summary>
        /// Frecuencia en segundos para escanear entradas expiradas
        /// </summary>
        public int ExpirationScanFrequencySeconds { get; set; } = 60;
    }

    public class PaginationSettings
    {
        /// <summary>
        /// Número máximo de elementos por página permitido
        /// </summary>
        public int MaxPageSize { get; set; } = 100;

        /// <summary>
        /// Número de elementos por página por defecto
        /// </summary>
        public int DefaultPageSize { get; set; } = 10;

        /// <summary>
        /// Página mínima permitida
        /// </summary>
        public int MinPage { get; set; } = 1;
    }

    public class CircuitBreakerSettings
    {
        /// <summary>
        /// Número de fallos consecutivos antes de abrir el circuito
        /// </summary>
        public int FailureThreshold { get; set; } = 6;

        /// <summary>
        /// Duración en segundos que el circuito permanece abierto antes de intentar recuperarse
        /// </summary>
        public int BreakDurationSeconds { get; set; } = 30;

        /// <summary>
        /// Timeout en segundos para cada operación protegida por el circuit breaker
        /// </summary>
        public int TimeoutSeconds { get; set; } = 5;
    }

    public class PubSubSettings
    {
        /// <summary>
        /// Enable or disable Pub/Sub integration. When false, no connection to Google Cloud Pub/Sub is made.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Google Cloud Project ID
        /// </summary>
        public string? ProjectId { get; set; }

        /// <summary>
        /// Subscription ID for order events
        /// </summary>
        public string? SubscriptionId { get; set; }

        /// <summary>
        /// Path to service account credentials JSON file.
        /// If not specified, Application Default Credentials (ADC) will be used.
        /// </summary>
        public string? CredentialsPath { get; set; }

        /// <summary>
        /// Maximum time in seconds to wait for messages before returning empty response
        /// </summary>
        public int PullTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Acknowledgment deadline in seconds (how long before unacked messages are redelivered)
        /// </summary>
        public int AckDeadlineSeconds { get; set; } = 60;
    }

}

