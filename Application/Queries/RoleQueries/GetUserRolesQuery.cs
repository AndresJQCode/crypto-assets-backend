using Application.Dtos.Role;
using MediatR;

namespace Application.Queries.RoleQueries;

internal sealed record GetUserRolesQuery(Guid UserId) : IRequest<IEnumerable<RoleDto>>;
