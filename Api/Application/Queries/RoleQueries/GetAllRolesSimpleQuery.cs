using Api.Application.Dtos.Role;
using MediatR;

namespace Api.Application.Queries.RoleQueries;

internal sealed class GetAllRolesSimpleQuery : IRequest<IEnumerable<RoleDto>>
{
}
