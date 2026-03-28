namespace Api.Application.Dtos.ConnectorInstance;

internal sealed class ConnectorInstanceDto
{
    public string Id { get; set; } = string.Empty;
    public string ConnectorDefinitionId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ProviderType { get; set; } = string.Empty;
    public string CategoryType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool IsConfigured { get; set; }
    public string? ConfigurationJson { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
