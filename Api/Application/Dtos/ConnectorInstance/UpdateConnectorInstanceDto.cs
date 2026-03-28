namespace Api.Application.Dtos.ConnectorInstance;

internal sealed class UpdateConnectorInstanceDto
{
    public string Name { get; set; } = string.Empty;
    public string? ConfigurationJson { get; set; }
}
