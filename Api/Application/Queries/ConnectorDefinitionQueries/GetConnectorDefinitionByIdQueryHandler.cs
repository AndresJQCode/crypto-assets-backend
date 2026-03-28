using Api.Application.Dtos.ConnectorDefinition;
using Domain.AggregatesModel.ConnectorDefinitionAggregate;
using MediatR;

namespace Api.Application.Queries.ConnectorDefinitionQueries;

internal sealed class GetConnectorDefinitionByIdQueryHandler(
    IConnectorDefinitionRepository repository) : IRequestHandler<GetConnectorDefinitionByIdQuery, ConnectorDefinitionDto?>
{
    public async Task<ConnectorDefinitionDto?> Handle(GetConnectorDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById(request.Id, cancellationToken: cancellationToken);
        if (entity is null || entity.IsDeleted)
            return null;

        return new ConnectorDefinitionDto
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            LogoUrl = entity.LogoUrl,
            ProviderType = entity.ProviderType,
            CategoryType = entity.CategoryType,
            IsActive = entity.IsActive,
            RequiresOAuth = entity.RequiresOAuth,
            Description = entity.Description
        };
    }
}
