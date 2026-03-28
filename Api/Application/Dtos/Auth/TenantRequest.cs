namespace Api.Application.Dtos.Auth;

/// <summary>
/// Datos del tenant/registro enviados en el state (URL-encoded JSON) en el flujo OAuth exchange-code.
/// </summary>
internal sealed class TenantRequest
{
    public string? TenantName { get; set; }
    public string? CountryName { get; set; }
    public string? CountryPhoneCode { get; set; }
    public string? WhatsAppNumber { get; set; }
    public required string Flow { get; set; }
}
