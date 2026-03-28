using Api.Application.Dtos.Tenant;
using Domain.AggregatesModel.TenantAggregate;
using MediatR;

namespace Api.Application.Queries.TenantQueries;

internal sealed class GetAllTenantsQueryHandler(ITenantRepository tenantRepository) : IRequestHandler<GetAllTenantsQuery, IReadOnlyList<TenantDto>>
{
    public async Task<IReadOnlyList<TenantDto>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await tenantRepository.GetByFilter(orderBy: q => q.OrderBy(t => t.Name), cancellationToken: cancellationToken);
        return tenants.Select(t => new TenantDto
        {
            Id = t.Id.ToString(),
            Name = t.Name,
            Slug = t.Slug,
            IsActive = t.IsActive
        }).ToList();
    }
}
