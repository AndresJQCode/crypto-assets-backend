namespace Api.Application.Dtos.ConnectorInstance;

public class CreateBybitConnectorDto
{
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public bool IsTestnet { get; set; }
}
