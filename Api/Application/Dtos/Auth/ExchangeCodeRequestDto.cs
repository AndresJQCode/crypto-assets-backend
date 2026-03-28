namespace Api.Application.Dtos.Auth;

internal sealed class ExchangeCodeRequestDto
{
    public required string Code { get; set; }
    public required string Provider { get; set; }
    /// <summary>State opcional: string URL-encoded que al decodificar es un JSON de <see cref="TenantRequest"/>.</summary>
    public string? State { get; set; }
}
