using System.Text.Json;
using Infrastructure.Constants;
using Infrastructure.Metrics;
using Infrastructure.Services.Auth.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Auth;

public class MicrosoftOAuthService(
    HttpClient httpClient,
    IOptionsMonitor<AppSettings> appSettings,
    ILogger<MicrosoftOAuthService> logger) : IMicrosoftOAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private AppSettings.MicrosoftOAuthSettings MicrosoftSettings => appSettings.CurrentValue.Authentication.Microsoft;

    public async Task<MicrosoftTokenResponse> ExchangeCodeForTokenAsync(string code)
    {
        try
        {
            var microsoftSettings = MicrosoftSettings;

            // Validar que tenemos toda la configuración necesaria
            if (microsoftSettings == null ||
                    string.IsNullOrEmpty(microsoftSettings.ClientId) ||
                    string.IsNullOrEmpty(microsoftSettings.ClientSecret) ||
                    string.IsNullOrEmpty(microsoftSettings.RedirectUri) ||
                    string.IsNullOrEmpty(microsoftSettings.TenantId))
            {
                logger.LogError("Configuración de Microsoft OAuth incompleta. Verificar ClientId, ClientSecret, RedirectUri y TenantId");

                // Métrica de Prometheus: configuración incompleta
                InfrastructureMetrics.AuthenticationAttemptsTotal
                    .WithLabels(MetricsLabelsConstants.OAuth.MicrosoftOAuth, MetricsLabelsConstants.OAuth.ConfigError)
                    .Inc();

                throw new InvalidOperationException("Configuración de Microsoft OAuth incompleta");
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Intercambiando código por token. TenantId: {TenantId}, ClientId: {ClientId}",
                    microsoftSettings.TenantId, microsoftSettings.ClientId);
            }

            using var requestBody = new FormUrlEncodedContent(new[]
            {
                                new KeyValuePair<string, string>("code", code),
                                new KeyValuePair<string, string>("client_id", microsoftSettings.ClientId),
                                new KeyValuePair<string, string>("client_secret", microsoftSettings.ClientSecret),
                                new KeyValuePair<string, string>("redirect_uri", microsoftSettings.RedirectUri),
                                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                                new KeyValuePair<string, string>("scope", "openid profile email User.Read")
                        });

            var tokenEndpoint = new Uri($"https://login.microsoftonline.com/{microsoftSettings.TenantId}/oauth2/v2.0/token");
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Enviando solicitud a: {TokenEndpoint}", tokenEndpoint);
            }

            var response = await httpClient.PostAsync(tokenEndpoint, requestBody);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Error en intercambio de token. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, responseContent);

                // Intentar deserializar el error de Microsoft
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<MicrosoftErrorResponse>(responseContent, JsonOptions);

                    if (errorResponse != null)
                    {
                        logger.LogError("Error de Microsoft: {Error} - {ErrorDescription}", errorResponse.Error, errorResponse.ErrorDescription);
                        throw new InvalidOperationException($"Error de Microsoft OAuth: {errorResponse.Error} - {errorResponse.ErrorDescription}");
                    }
                }
                catch (JsonException)
                {
                    // Si no se puede deserializar como error de Microsoft, lanzar error genérico
                }

                throw new HttpRequestException($"Error al intercambiar código por token. Status: {response.StatusCode}, Response: {responseContent}");
            }

            var tokenResponse = JsonSerializer.Deserialize<MicrosoftTokenResponse>(responseContent, JsonOptions);

            if (tokenResponse == null)
            {
                logger.LogError("No se pudo deserializar la respuesta del token de Microsoft");

                // Métrica de Prometheus: error en deserialización
                InfrastructureMetrics.AuthenticationAttemptsTotal
                    .WithLabels(MetricsLabelsConstants.OAuth.MicrosoftOAuth, MetricsLabelsConstants.OAuth.ExchangeError)
                    .Inc();

                throw new InvalidOperationException("No se pudo deserializar la respuesta del token de Microsoft");
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Token intercambiado exitosamente");
            }

            // Métrica de Prometheus: exchange exitoso de Microsoft
            InfrastructureMetrics.AuthenticationAttemptsTotal
                .WithLabels(MetricsLabelsConstants.OAuth.MicrosoftOAuth, MetricsLabelsConstants.OAuth.ExchangeSuccess)
                .Inc();

            return tokenResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al intercambiar código por token con Microsoft");

            // Métrica de Prometheus: error en exchange de Microsoft
            InfrastructureMetrics.AuthenticationAttemptsTotal
                .WithLabels(MetricsLabelsConstants.OAuth.MicrosoftOAuth, MetricsLabelsConstants.OAuth.ExchangeFailed)
                .Inc();

            throw;
        }
    }

    public async Task<MicrosoftUserInfo> GetUserInfoAsync(string accessToken)
    {
        try
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentException("El token de acceso no puede estar vacío", nameof(accessToken));
            }

            // Limpiar headers previos
            httpClient.DefaultRequestHeaders.Authorization = null;

            // Establecer el header de autorización
            httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Obteniendo información del usuario de Microsoft Graph");
            }

            var response = await httpClient.GetAsync(new Uri("https://graph.microsoft.com/v1.0/me"));

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Error al obtener información del usuario. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, responseContent);

                // Limpiar el header de autorización en caso de error
                httpClient.DefaultRequestHeaders.Authorization = null;

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Token de acceso inválido o expirado");
                }

                throw new HttpRequestException($"Error al obtener información del usuario. Status: {response.StatusCode}, Response: {responseContent}");
            }

            var userInfo = JsonSerializer.Deserialize<MicrosoftUserInfo>(responseContent, JsonOptions);

            if (userInfo == null)
            {
                logger.LogError("No se pudo deserializar la información del usuario de Microsoft");

                // Métrica de Prometheus: error al obtener info de usuario
                InfrastructureMetrics.AuthenticationAttemptsTotal
                    .WithLabels(MetricsLabelsConstants.OAuth.MicrosoftOAuth, MetricsLabelsConstants.OAuth.UserInfoError)
                    .Inc();

                throw new InvalidOperationException("No se pudo deserializar la información del usuario de Microsoft");
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Información del usuario obtenida exitosamente para: {UserPrincipalName}", userInfo.UserPrincipalName);
            }

            // Métrica de Prometheus: info de usuario obtenida exitosamente
            InfrastructureMetrics.AuthenticationAttemptsTotal
                .WithLabels(MetricsLabelsConstants.OAuth.MicrosoftOAuth, MetricsLabelsConstants.OAuth.UserInfoSuccess)
                .Inc();

            // Limpiar el header de autorización después de usar
            httpClient.DefaultRequestHeaders.Authorization = null;

            return userInfo;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener información del usuario de Microsoft");

            // Métrica de Prometheus: error al obtener info de usuario
            InfrastructureMetrics.AuthenticationAttemptsTotal
                .WithLabels(MetricsLabelsConstants.OAuth.MicrosoftOAuth, MetricsLabelsConstants.OAuth.UserInfoFailed)
                .Inc();

            // Asegurar que se limpia el header de autorización en caso de error
            httpClient.DefaultRequestHeaders.Authorization = null;
            throw;
        }
    }
}
