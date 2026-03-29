using Application.Dtos.User;
using MediatR;

namespace Application.Queries.RoleQueries;

internal sealed record GetRoleUsersQuery(Guid RoleId) : IRequest<IEnumerable<UserResponseDto>>;
