using Application.Dtos.Role;
using MediatR;

namespace Application.Queries.RoleQueries;

internal sealed record GetRoleByIdQuery(Guid Id) : IRequest<RoleDto?>;
