using Api.Application.Dtos.Role;
using MediatR;

namespace Api.Application.Queries.RoleQueries;

internal sealed record GetUserRolesQuery(Guid UserId) : IRequest<IEnumerable<RoleDto>>;
