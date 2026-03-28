using Api.Application.Dtos.ConnectorInstance;
using MediatR;

namespace Api.Application.Commands.ConnectorInstanceCommands;

internal sealed class UpdateConnectorInstanceCommand : IRequest<ConnectorInstanceDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ConfigurationJson { get; set; }
}
