using Api.Application.Dtos.ConnectorDefinition;
using MediatR;

namespace Api.Application.Commands.ConnectorDefinitionCommands;

internal sealed class CreateConnectorDefinitionCommand : IRequest<ConnectorDefinitionDto>
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string CategoryType { get; set; } = string.Empty;
    public bool RequiresOAuth { get; set; }
    public string? Description { get; set; }
}
