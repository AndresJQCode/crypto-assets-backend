using Api.Application.Dtos;
using Api.Application.Dtos.ConnectorDefinition;
using Api.Utilities;
using Domain.AggregatesModel.ConnectorDefinitionAggregate;
using Domain.SeedWork;
using Mapster;
using MediatR;

namespace Api.Application.Queries.ConnectorDefinitionQueries;

internal sealed class GetAllConnectorDefinitionsQueryHandler(
    IConnectorDefinitionRepository repository) : IRequestHandler<GetAllConnectorDefinitionsQuery, PaginationResponseDto<ConnectorDefinitionDto>>
{
    public async Task<PaginationResponseDto<ConnectorDefinitionDto>> Handle(GetAllConnectorDefinitionsQuery request, CancellationToken cancellationToken)
    {
        PaginationParameters? p = request.PaginationParameters;
        PagedResult<ConnectorDefinition> paged = await repository.GetByFilterPagination(
            filter: cd => !cd.IsDeleted,
            orderBy: q => q.OrderBy(cd => cd.Name),
            page: p.Page,
            pageSize: p.Limit,
            cancellationToken: cancellationToken);

        IReadOnlyList<ConnectorDefinitionDto> data = paged.Items.Adapt<IReadOnlyList<ConnectorDefinitionDto>>();
        return new PaginationResponseDto<ConnectorDefinitionDto>
        {
            Data = data,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
            Limit = p.Limit,
            Page = p.Page
        };
    }
}
