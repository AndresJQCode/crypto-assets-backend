using Api.Application.Dtos.Role;
using MediatR;

namespace Api.Application.Queries.RoleQueries;

internal sealed record GetRoleByIdQuery(Guid Id) : IRequest<RoleDto?>;
