using Api.Application.Dtos;
using Api.Application.Dtos.ConnectorDefinition;
using Api.Utilities;
using MediatR;

namespace Api.Application.Queries.ConnectorDefinitionQueries;

internal sealed class GetAllConnectorDefinitionsQuery
    : IRequest<PaginationResponseDto<ConnectorDefinitionDto>>,
      IPaginatedQuery<PaginationResponseDto<ConnectorDefinitionDto>>
{
    public PaginationParameters PaginationParameters { get; set; } = new();
}
