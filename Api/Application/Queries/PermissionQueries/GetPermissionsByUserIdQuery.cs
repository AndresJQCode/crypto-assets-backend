using Api.Application.Dtos.Permission;
using MediatR;

namespace Api.Application.Queries.PermissionQueries;

internal sealed record GetPermissionsByUserIdQuery(Guid UserId) : IRequest<IEnumerable<PermissionDto>>;
