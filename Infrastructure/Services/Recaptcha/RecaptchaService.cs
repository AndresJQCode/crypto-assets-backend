using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Recaptcha;

/// <summary>
/// Servicio para validar tokens de Google reCAPTCHA v3
/// </summary>
public class RecaptchaService(
    IOptionsMonitor<AppSettings> appSettings,
    IHttpClientFactory httpClientFactory,
    ILogger<RecaptchaService> logger) : IRecaptchaService
{
    private AppSettings.RecaptchaSettings Settings => appSettings.CurrentValue.Recaptcha;

    public async Task<RecaptchaValidationResult> ValidateTokenAsync(string recaptchaToken, string? remoteIpAddress = null)
    {
        // Si reCAPTCHA está deshabilitado, permitir la validación
        if (!Settings.Enabled)
        {
            logger.LogDebug("reCAPTCHA está deshabilitado, saltando validación");
            return new RecaptchaValidationResult
            {
                Success = true,
                Score = 1.0
            };
        }

        if (string.IsNullOrWhiteSpace(recaptchaToken))
        {
            logger.LogWarning("Token de reCAPTCHA vacío o nulo");
            return new RecaptchaValidationResult
            {
                Success = false,
                Score = 0.0,
                ErrorMessage = "Token de reCAPTCHA requerido",
                ErrorCodes = new List<string> { "missing-input-response" }
            };
        }

        try
        {
            // Construir la URL de verificación
            var requestUri = $"{Settings.VerificationUrl}?secret={Settings.SecretKey}&response={recaptchaToken}";

            if (!string.IsNullOrWhiteSpace(remoteIpAddress))
            {
                requestUri += $"&remoteip={remoteIpAddress}";
            }

            logger.LogDebug("Validando token de reCAPTCHA con Google");

            // Realizar la petición a Google
            using var httpClient = httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(Settings.TimeoutSeconds);
            var response = await httpClient.PostAsync(new Uri(requestUri), null);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadFromJsonAsync<GoogleRecaptchaResponse>();

            if (jsonResponse == null)
            {
                logger.LogError("Respuesta inválida de Google reCAPTCHA");
                return new RecaptchaValidationResult
                {
                    Success = false,
                    Score = 0.0,
                    ErrorMessage = "Error al validar con Google reCAPTCHA",
                    ErrorCodes = new List<string> { "invalid-response" }
                };
            }

            // Verificar si la validación fue exitosa
            if (!jsonResponse.Success)
            {
                logger.LogWarning("Validación de reCAPTCHA fallida. Errores: {Errors}",
                    string.Join(", ", jsonResponse.ErrorCodes ?? new List<string>()));

                return new RecaptchaValidationResult
                {
                    Success = false,
                    Score = 0.0,
                    ErrorMessage = "Token de reCAPTCHA inválido",
                    ErrorCodes = jsonResponse.ErrorCodes ?? new List<string>()
                };
            }

            // Verificar el score (solo para reCAPTCHA v3)
            var score = jsonResponse.Score ?? 0.0;
            if (score < Settings.MinimumScore)
            {
                logger.LogWarning("Score de reCAPTCHA demasiado bajo: {Score} (mínimo requerido: {MinimumScore})",
                    score, Settings.MinimumScore);

                return new RecaptchaValidationResult
                {
                    Success = false,
                    Score = score,
                    ErrorMessage = $"Score de reCAPTCHA demasiado bajo: {score:F2} (mínimo: {Settings.MinimumScore:F2})",
                    ErrorCodes = new List<string> { "low-score" }
                };
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Validación de reCAPTCHA exitosa. Score: {Score}", score);
            }

            return new RecaptchaValidationResult
            {
                Success = true,
                Score = score,
                ErrorCodes = new List<string>()
            };
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout al validar token de reCAPTCHA");
            return new RecaptchaValidationResult
            {
                Success = false,
                Score = 0.0,
                ErrorMessage = "Timeout al validar con Google reCAPTCHA",
                ErrorCodes = new List<string> { "timeout" }
            };
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error HTTP al validar token de reCAPTCHA");
            return new RecaptchaValidationResult
            {
                Success = false,
                Score = 0.0,
                ErrorMessage = "Error al comunicarse con Google reCAPTCHA",
                ErrorCodes = new List<string> { "http-error" }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado al validar token de reCAPTCHA");
            return new RecaptchaValidationResult
            {
                Success = false,
                Score = 0.0,
                ErrorMessage = "Error inesperado al validar reCAPTCHA",
                ErrorCodes = new List<string> { "unexpected-error" }
            };
        }
    }

    /// <summary>
    /// Respuesta de la API de Google reCAPTCHA
    /// </summary>
    private sealed class GoogleRecaptchaResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("score")]
        public double? Score { get; set; }

        [JsonPropertyName("action")]
        public string? Action { get; set; }

        [JsonPropertyName("challenge_ts")]
        public DateTime? ChallengeTimestamp { get; set; }

        [JsonPropertyName("hostname")]
        public string? Hostname { get; set; }

        [JsonPropertyName("error-codes")]
        public List<string>? ErrorCodes { get; set; }
    }
}

