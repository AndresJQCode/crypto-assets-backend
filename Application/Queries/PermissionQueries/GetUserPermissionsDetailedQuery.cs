using Application.Dtos.Permission;
using MediatR;

namespace Application.Queries.PermissionQueries;

internal sealed record GetUserPermissionsDetailedQuery(Guid UserId) : IRequest<IEnumerable<UserPermissionDto>>;
