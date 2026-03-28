using MediatR;

namespace Api.Application.Commands.ConnectorInstanceCommands;

internal sealed class DeleteConnectorInstanceCommand(Guid id) : IRequest<Unit>
{
    public Guid Id { get; } = id;
}
