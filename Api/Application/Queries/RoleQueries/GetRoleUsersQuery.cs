using Api.Application.Dtos.User;
using MediatR;

namespace Api.Application.Queries.RoleQueries;

internal sealed record GetRoleUsersQuery(Guid RoleId) : IRequest<IEnumerable<UserResponseDto>>;
