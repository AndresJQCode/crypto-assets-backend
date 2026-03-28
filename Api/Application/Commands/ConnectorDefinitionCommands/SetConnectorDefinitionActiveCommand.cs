using Api.Application.Dtos.ConnectorDefinition;
using MediatR;

namespace Api.Application.Commands.ConnectorDefinitionCommands;

internal sealed class SetConnectorDefinitionActiveCommand(Guid id, bool isActive) : IRequest<ConnectorDefinitionDto>
{
    public Guid Id { get; } = id;
    public bool IsActive { get; } = isActive;
}
