using Api.Application.Dtos.ConnectorDefinition;
using MediatR;

namespace Api.Application.Queries.ConnectorDefinitionQueries;

internal sealed class GetConnectorDefinitionByIdQuery(Guid id) : IRequest<ConnectorDefinitionDto?>
{
    public Guid Id { get; } = id;
}
