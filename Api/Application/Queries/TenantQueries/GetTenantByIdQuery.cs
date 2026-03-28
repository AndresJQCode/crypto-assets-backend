using Api.Application.Dtos.Tenant;
using MediatR;

namespace Api.Application.Queries.TenantQueries;

internal sealed class GetTenantByIdQuery(Guid id) : IRequest<TenantDto?>
{
    public Guid Id { get; } = id;
}
