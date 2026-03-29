using Application.Dtos.Role;
using MediatR;

namespace Application.Queries.RoleQueries;

internal sealed record GetRoleByIdWithPermissionsQuery(Guid Id) : IRequest<RoleWithPermissionsDto?>;
