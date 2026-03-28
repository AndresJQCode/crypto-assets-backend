using Domain.AggregatesModel.PermissionAggregate;
using MediatR;

namespace Api.Application.Queries.RoleQueries;

internal sealed record GetRolePermissionsQuery(Guid RoleId) : IRequest<IEnumerable<Permission>>;
