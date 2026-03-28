namespace Api.Application.Dtos.ConnectorDefinition;

internal sealed class ConnectorDefinitionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string CategoryType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool RequiresOAuth { get; set; }
    public string? Description { get; set; }
}
