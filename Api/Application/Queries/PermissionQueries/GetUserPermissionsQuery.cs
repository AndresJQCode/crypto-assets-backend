using Domain.AggregatesModel.PermissionAggregate;
using MediatR;

namespace Api.Application.Queries.PermissionQueries;

internal sealed record GetUserPermissionsQuery(Guid UserId) : IRequest<IEnumerable<Permission>>;
