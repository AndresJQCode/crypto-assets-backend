using Domain.AggregatesModel.PermissionAggregate;
using MediatR;

namespace Application.Queries.PermissionQueries;

internal sealed record GetUserPermissionsQuery(Guid UserId) : IRequest<IEnumerable<Permission>>;
