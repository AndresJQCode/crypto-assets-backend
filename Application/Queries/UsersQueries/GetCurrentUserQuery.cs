using Application.Dtos.User;
using MediatR;

namespace Application.Queries.Users;

internal sealed class GetCurrentUserQuery : IRequest<CurrentUserDto>
{
}
