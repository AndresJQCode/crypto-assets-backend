using Api.Application.Dtos.Role;
using MediatR;

namespace Api.Application.Queries.RoleQueries;

internal sealed class GetAllRolesWithPermissionsQuery : IRequest<IEnumerable<RoleWithPermissionsDto>>
{
}
