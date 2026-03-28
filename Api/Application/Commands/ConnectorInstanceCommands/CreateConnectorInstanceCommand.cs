using Api.Application.Dtos.ConnectorInstance;
using MediatR;

namespace Api.Application.Commands.ConnectorInstanceCommands;

internal sealed class CreateConnectorInstanceCommand : IRequest<ConnectorInstanceDto>
{
    public Guid ConnectorDefinitionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ConfigurationJson { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}
