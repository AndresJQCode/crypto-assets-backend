using Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Api.Infrastructure.HealthChecks;

internal sealed class EmailServiceHealthCheck(
    IOptionsMonitor<AppSettings> appSettings,
    ILogger<EmailServiceHealthCheck> logger) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var emailSettings = appSettings.CurrentValue.EmailService;
            var infobipSettings = appSettings.CurrentValue.Infobip;

            // Verificar solo configuración
            if (string.IsNullOrEmpty(infobipSettings?.ApiKey))
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("ApiKey no configurada"));
            }

            if (string.IsNullOrEmpty(emailSettings?.FromEmail))
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("FromEmail no configurado"));
            }

            return Task.FromResult(HealthCheckResult.Healthy("Email configurado"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al verificar configuración de Email");
            return Task.FromResult(HealthCheckResult.Unhealthy("Error en configuración Email", exception: ex));
        }
    }
}

