using Api.Application.Dtos.Role;
using MediatR;

namespace Api.Application.Queries.RoleQueries;

internal sealed record GetRoleByIdWithPermissionsQuery(Guid Id) : IRequest<RoleWithPermissionsDto?>;
