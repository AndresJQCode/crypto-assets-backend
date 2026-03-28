using Api.Application.Dtos.User;
using MediatR;

namespace Api.Application.Queries.Users;

internal sealed record GetUserByIdQuery(Guid Id) : IRequest<UserResponseDto>;
