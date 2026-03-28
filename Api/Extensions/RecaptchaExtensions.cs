using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Services.Recaptcha;
using Microsoft.Extensions.Options;

namespace Api.Extensions;

/// <summary>
/// Extensiones para configurar reCAPTCHA
/// </summary>
internal static class RecaptchaExtensions
{
    /// <summary>
    /// Agregar servicios de reCAPTCHA
    /// </summary>
    public static IServiceCollection AddRecaptchaServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar RecaptchaSettings desde la configuración
        services.Configure<AppSettings.RecaptchaSettings>(configuration.GetSection("Recaptcha"));

        // Registrar HttpClient factory (necesario para IHttpClientFactory)
        // AddHttpClient() es idempotente, se puede llamar múltiples veces sin problema
        services.AddHttpClient();

        // Registrar el servicio de reCAPTCHA siempre
        // El servicio internamente verifica si está habilitado y retorna Success=true si está deshabilitado
        services.AddScoped<IRecaptchaService, RecaptchaService>();

        return services;
    }
}

