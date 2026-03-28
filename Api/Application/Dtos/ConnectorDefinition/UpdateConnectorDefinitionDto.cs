namespace Api.Application.Dtos.ConnectorDefinition;

internal sealed class UpdateConnectorDefinitionDto
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
}
