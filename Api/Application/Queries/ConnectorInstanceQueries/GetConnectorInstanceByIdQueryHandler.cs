using Api.Application.Dtos.ConnectorInstance;
using Api.Infrastructure.Services;
using Domain.AggregatesModel.ConnectorInstanceAggregate;
using MediatR;

namespace Api.Application.Queries.ConnectorInstanceQueries;

internal sealed class GetConnectorInstanceByIdQueryHandler(
    IConnectorInstanceRepository repository,
    ITenantContext tenantContext) : IRequestHandler<GetConnectorInstanceByIdQuery, ConnectorInstanceDto?>
{
    public async Task<ConnectorInstanceDto?> Handle(GetConnectorInstanceByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById(request.Id, cancellationToken: cancellationToken);
        if (entity is null || entity.IsDeleted)
            return null;

        var tenantId = tenantContext.GetCurrentTenantId();
        if (tenantId.HasValue && entity.TenantId != tenantId.Value)
            return null;

        return new ConnectorInstanceDto
        {
            Id = entity.Id.ToString(),
            ConnectorDefinitionId = entity.ConnectorDefinitionId.ToString(),
            TenantId = entity.TenantId.ToString(),
            UserId = entity.UserId.ToString(),
            ProviderType = entity.ProviderType.ToString(),
            CategoryType = entity.CategoryType.ToString(),
            Name = entity.Name,
            IsEnabled = entity.IsEnabled,
            IsConfigured = entity.IsConfigured,
            ConfigurationJson = entity.ConfigurationJson,
            LastSyncedAt = entity.LastSyncedAt,
            Status = entity.GetStatus().ToString()
        };
    }
}
