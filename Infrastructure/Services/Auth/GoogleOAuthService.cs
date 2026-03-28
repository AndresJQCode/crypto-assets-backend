using System.Text.Json;
using System.Text.Json.Serialization;
using Infrastructure;
using Infrastructure.Constants;
using Infrastructure.Metrics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Auth;

public interface IGoogleOAuthService
{
    Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string code);
    Task<GoogleUserInfo> GetUserInfoAsync(string accessToken);
}

public class GoogleOAuthService(
    HttpClient httpClient,
    IOptionsMonitor<AppSettings> appSettings,
    ILogger<GoogleOAuthService> logger) : IGoogleOAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private AppSettings.GoogleOAuthSettings GoogleSettings => appSettings.CurrentValue.Authentication.Google;

    public async Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string code)
    {
        try
        {
            var clientId = GoogleSettings.ClientId;
            var clientSecret = GoogleSettings.ClientSecret;
            var redirectUri = GoogleSettings.RedirectUri;

            using var requestBody = new FormUrlEncodedContent(new[]
            {
                                new KeyValuePair<string, string>("code", code),
                                new KeyValuePair<string, string>("client_id", clientId),
                                new KeyValuePair<string, string>("client_secret", clientSecret),
                                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                                new KeyValuePair<string, string>("grant_type", "authorization_code")
                        });

            var response = await httpClient.PostAsync(new Uri("https://oauth2.googleapis.com/token"), requestBody);
            // response.EnsureSuccessStatusCode();
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Error al intercambiar código por token con Google: {StatusCode}", response.StatusCode);
                throw new HttpRequestException($"Error al intercambiar código por token con Google: {response.StatusCode}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(jsonResponse, JsonOptions);

            if (tokenResponse == null)
            {
                logger.LogError("No se pudo deserializar la respuesta del token de Google");

                // Métrica de Prometheus: error en exchange de Google
                InfrastructureMetrics.AuthenticationAttemptsTotal
                    .WithLabels(MetricsLabelsConstants.OAuth.GoogleOAuth, MetricsLabelsConstants.OAuth.ExchangeError)
                    .Inc();

                throw new InvalidOperationException("No se pudo deserializar la respuesta del token de Google");
            }

            // Métrica de Prometheus: exchange exitoso de Google
            InfrastructureMetrics.AuthenticationAttemptsTotal
                .WithLabels(MetricsLabelsConstants.OAuth.GoogleOAuth, MetricsLabelsConstants.OAuth.ExchangeSuccess)
                .Inc();

            return tokenResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al intercambiar código por token con Google");

            // Métrica de Prometheus: error en exchange de Google
            InfrastructureMetrics.AuthenticationAttemptsTotal
                .WithLabels(MetricsLabelsConstants.OAuth.GoogleOAuth, MetricsLabelsConstants.OAuth.ExchangeFailed)
                .Inc();

            throw;
        }
    }

    public async Task<GoogleUserInfo> GetUserInfoAsync(string accessToken)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync(new Uri("https://www.googleapis.com/oauth2/v2/userinfo"));
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(jsonResponse, JsonOptions);

            if (userInfo == null)
            {
                logger.LogError("No se pudo deserializar la información del usuario de Google");

                // Métrica de Prometheus: error al obtener info de usuario de Google
                InfrastructureMetrics.AuthenticationAttemptsTotal
                    .WithLabels(MetricsLabelsConstants.OAuth.GoogleOAuth, MetricsLabelsConstants.OAuth.UserInfoError)
                    .Inc();

                throw new InvalidOperationException("No se pudo deserializar la información del usuario de Google");
            }

            // Métrica de Prometheus: info de usuario obtenida exitosamente
            InfrastructureMetrics.AuthenticationAttemptsTotal
                .WithLabels(MetricsLabelsConstants.OAuth.GoogleOAuth, MetricsLabelsConstants.OAuth.UserInfoSuccess)
                .Inc();

            return userInfo;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener información del usuario de Google");

            // Métrica de Prometheus: error al obtener info de usuario de Google
            InfrastructureMetrics.AuthenticationAttemptsTotal
                .WithLabels(MetricsLabelsConstants.OAuth.GoogleOAuth, MetricsLabelsConstants.OAuth.UserInfoFailed)
                .Inc();

            throw;
        }
    }
}

public class GoogleTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}

public class GoogleUserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public bool VerifiedEmail { get; set; }
}
