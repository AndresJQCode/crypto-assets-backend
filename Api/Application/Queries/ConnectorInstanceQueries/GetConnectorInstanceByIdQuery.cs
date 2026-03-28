using Api.Application.Dtos.ConnectorInstance;
using MediatR;

namespace Api.Application.Queries.ConnectorInstanceQueries;

internal sealed class GetConnectorInstanceByIdQuery(Guid id) : IRequest<ConnectorInstanceDto?>
{
    public Guid Id { get; } = id;
}
