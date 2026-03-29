using Domain.AggregatesModel.PermissionAggregate;
using MediatR;

namespace Application.Queries.RoleQueries;

internal sealed record GetRolePermissionsQuery(Guid RoleId) : IRequest<IEnumerable<Permission>>;
