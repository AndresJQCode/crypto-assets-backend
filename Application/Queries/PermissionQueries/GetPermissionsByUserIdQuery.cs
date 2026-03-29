using Application.Dtos.Permission;
using MediatR;

namespace Application.Queries.PermissionQueries;

internal sealed record GetPermissionsByUserIdQuery(Guid UserId) : IRequest<IEnumerable<PermissionDto>>;
