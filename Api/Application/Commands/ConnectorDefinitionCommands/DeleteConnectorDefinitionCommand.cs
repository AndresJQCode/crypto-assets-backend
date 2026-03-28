using MediatR;

namespace Api.Application.Commands.ConnectorDefinitionCommands;

internal sealed class DeleteConnectorDefinitionCommand(Guid id) : IRequest<Unit>
{
    public Guid Id { get; } = id;
}
