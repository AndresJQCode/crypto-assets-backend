using Api.Application.Dtos.ConnectorDefinition;
using MediatR;

namespace Api.Application.Commands.ConnectorDefinitionCommands;

internal sealed class UpdateConnectorDefinitionCommand : IRequest<ConnectorDefinitionDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
}
