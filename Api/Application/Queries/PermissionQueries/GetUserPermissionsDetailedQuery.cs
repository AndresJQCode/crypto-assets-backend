using Api.Application.Dtos.Permission;
using MediatR;

namespace Api.Application.Queries.PermissionQueries;

internal sealed record GetUserPermissionsDetailedQuery(Guid UserId) : IRequest<IEnumerable<UserPermissionDto>>;
