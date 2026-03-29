using Application.Dtos.User;
using MediatR;

namespace Application.Queries.Users;

internal sealed record GetUserByIdQuery(Guid Id) : IRequest<UserResponseDto>;
