using Application.Dtos.Role;
using MediatR;

namespace Application.Queries.RoleQueries;

internal sealed class GetAllRolesSimpleQuery : IRequest<IEnumerable<RoleDto>>
{
}
