using Api.Application.Dtos.ConnectorInstance;
using MediatR;

namespace Api.Application.Commands.ConnectorInstanceCommands;

internal sealed class SetConnectorInstanceEnabledCommand(Guid id, bool isEnabled) : IRequest<ConnectorInstanceDto>
{
    public Guid Id { get; } = id;
    public bool IsEnabled { get; } = isEnabled;
}
