using System.Text.Json.Serialization;

namespace Infrastructure.Services.Auth.Dtos;

public class MicrosoftTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    [JsonPropertyName("ext_expires_in")]
    public int ExtExpiresIn { get; set; }
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
    [JsonPropertyName("id_token")]
    public string IdToken { get; set; } = string.Empty;
}

public class MicrosoftUserInfo
{
    public string ODataContext { get; set; } = string.Empty;
    public string[] BusinessPhones { get; set; } = Array.Empty<string>();
    public string DisplayName { get; set; } = string.Empty;
    public string? GivenName { get; set; }
    public string? JobTitle { get; set; }
    public string Mail { get; set; } = string.Empty;
    public string? MobilePhone { get; set; }
    public string? OfficeLocation { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? Surname { get; set; }
    public string UserPrincipalName { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
}

public class MicrosoftErrorResponse
{
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;

    [JsonPropertyName("error_description")]
    public string ErrorDescription { get; set; } = string.Empty;

    [JsonPropertyName("error_codes")]
    public int[] ErrorCodes { get; set; } = Array.Empty<int>();

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("trace_id")]
    public string TraceId { get; set; } = string.Empty;

    [JsonPropertyName("correlation_id")]
    public string CorrelationId { get; set; } = string.Empty;

    [JsonPropertyName("error_uri")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI properties should not be strings", Justification = "This is a DTO for Microsoft API response, must match their JSON structure")]
    public string ErrorUri { get; set; } = string.Empty;
}
