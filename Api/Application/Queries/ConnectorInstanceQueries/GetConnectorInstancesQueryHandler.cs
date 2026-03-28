using Api.Application.Dtos.ConnectorInstance;
using Api.Infrastructure.Services;
using Domain.AggregatesModel.ConnectorInstanceAggregate;
using MediatR;

namespace Api.Application.Queries.ConnectorInstanceQueries;

internal sealed class GetConnectorInstancesQueryHandler(
    IConnectorInstanceRepository repository,
    ITenantContext tenantContext) : IRequestHandler<GetConnectorInstancesQuery, IReadOnlyList<ConnectorInstanceDto>>
{
    public async Task<IReadOnlyList<ConnectorInstanceDto>> Handle(GetConnectorInstancesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.GetCurrentTenantId();
        if (!tenantId.HasValue)
            return [];

        var list = await repository.GetByTenantAsync(tenantId.Value, cancellationToken);
        return list.Where(c => !c.IsDeleted).Select(MapToDto).ToList();
    }

    private static ConnectorInstanceDto MapToDto(ConnectorInstance e) => new()
    {
        Id = e.Id.ToString(),
        ConnectorDefinitionId = e.ConnectorDefinitionId.ToString(),
        TenantId = e.TenantId.ToString(),
        UserId = e.UserId.ToString(),
        ProviderType = e.ProviderType.ToString(),
        CategoryType = e.CategoryType.ToString(),
        Name = e.Name,
        IsEnabled = e.IsEnabled,
        IsConfigured = e.IsConfigured,
        ConfigurationJson = e.ConfigurationJson,
        LastSyncedAt = e.LastSyncedAt,
        Status = e.GetStatus().ToString()
    };
}
