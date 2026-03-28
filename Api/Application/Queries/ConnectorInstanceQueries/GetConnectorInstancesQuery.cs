using Api.Application.Dtos.ConnectorInstance;
using MediatR;

namespace Api.Application.Queries.ConnectorInstanceQueries;

/// <summary>
/// Gets connector instances for the current tenant (from context).
/// </summary>
internal sealed class GetConnectorInstancesQuery : IRequest<IReadOnlyList<ConnectorInstanceDto>>;
