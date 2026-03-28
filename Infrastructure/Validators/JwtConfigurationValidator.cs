using System.Text;
using Microsoft.Extensions.Options;

namespace Infrastructure.Validators;

/// <summary>
/// Validador para la configuración de JWT que se ejecuta al inicio de la aplicación.
/// Garantiza que la SecretKey cumpla con los requisitos mínimos de seguridad.
/// </summary>
public class JwtConfigurationValidator : IValidateOptions<AppSettings>
{
    public ValidateOptionsResult Validate(string? name, AppSettings options)
    {
        var jwtSettings = options.JwtSettings;

        // Validar que SecretKey no esté vacía
        if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
        {
            return ValidateOptionsResult.Fail("JWT SecretKey no está configurado o está vacío.");
        }

        // Validar longitud mínima de 256 bits (32 bytes) para HMAC-SHA256
        var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
        if (keyBytes.Length < 32)
        {
            return ValidateOptionsResult.Fail(
                $"JWT SecretKey debe tener al menos 256 bits (32 bytes) para HMAC-SHA256. " +
                $"Longitud actual: {keyBytes.Length} bytes ({keyBytes.Length * 8} bits). " +
                $"Considere usar una clave generada criptográficamente segura.");
        }

        // Validar que Issuer no esté vacío
        if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
        {
            return ValidateOptionsResult.Fail("JWT Issuer no está configurado o está vacío.");
        }

        // Validar que Audience no esté vacía
        if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
        {
            return ValidateOptionsResult.Fail("JWT Audience no está configurado o está vacío.");
        }

        // Validar que ExpirationMinutes sea razonable (entre 1 minuto y 24 horas)
        if (jwtSettings.ExpirationMinutes < 1 || jwtSettings.ExpirationMinutes > 1440)
        {
            return ValidateOptionsResult.Fail(
                $"JWT ExpirationMinutes debe estar entre 1 y 1440 (24 horas). Valor actual: {jwtSettings.ExpirationMinutes}");
        }

        // Validar que RefreshTokenExpirationDays sea razonable (entre 1 y 90 días)
        if (jwtSettings.RefreshTokenExpirationDays < 1 || jwtSettings.RefreshTokenExpirationDays > 90)
        {
            return ValidateOptionsResult.Fail(
                $"JWT RefreshTokenExpirationDays debe estar entre 1 y 90 días. Valor actual: {jwtSettings.RefreshTokenExpirationDays}");
        }

        return ValidateOptionsResult.Success;
    }
}
