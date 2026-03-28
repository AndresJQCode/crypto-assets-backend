using Api.Application.Dtos.Tenant;
using MediatR;

namespace Api.Application.Queries.TenantQueries;

internal sealed class GetAllTenantsQuery : IRequest<IReadOnlyList<TenantDto>>;
