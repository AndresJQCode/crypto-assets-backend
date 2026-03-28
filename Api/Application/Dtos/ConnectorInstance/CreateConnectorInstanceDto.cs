namespace Api.Application.Dtos.ConnectorInstance;

/// <summary>
/// DTO for creating a connector instance (e.g. after OAuth callback).
/// AccessToken is encrypted before storage.
/// </summary>
internal sealed class CreateConnectorInstanceDto
{
    public Guid ConnectorDefinitionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ConfigurationJson { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}
