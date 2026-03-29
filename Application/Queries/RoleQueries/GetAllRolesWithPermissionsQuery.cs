using Application.Dtos.Role;
using MediatR;

namespace Application.Queries.RoleQueries;

internal sealed class GetAllRolesWithPermissionsQuery : IRequest<IEnumerable<RoleWithPermissionsDto>>
{
}
