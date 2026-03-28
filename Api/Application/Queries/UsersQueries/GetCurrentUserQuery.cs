using Api.Application.Dtos.User;
using MediatR;

namespace Api.Application.Queries.Users;

internal sealed class GetCurrentUserQuery : IRequest<CurrentUserDto>
{
}
