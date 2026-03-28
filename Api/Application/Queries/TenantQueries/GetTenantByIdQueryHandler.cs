using Api.Application.Dtos.Tenant;
using Domain.AggregatesModel.TenantAggregate;
using MediatR;

namespace Api.Application.Queries.TenantQueries;

internal sealed class GetTenantByIdQueryHandler(ITenantRepository tenantRepository) : IRequestHandler<GetTenantByIdQuery, TenantDto?>
{
    public async Task<TenantDto?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetById(request.Id, cancellationToken: cancellationToken);
        return tenant is null ? null : new TenantDto
        {
            Id = tenant.Id.ToString(),
            Name = tenant.Name,
            Slug = tenant.Slug,
            IsActive = tenant.IsActive
        };
    }
}
