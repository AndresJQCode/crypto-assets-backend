using Application.Dtos.User;
using MediatR;

namespace Application.Commands.UserCommands;

internal sealed record UpdateUserCommand(Guid Id, string Name, string Email, IReadOnlyCollection<string>? RoleIds) : IRequest<UserResponseDto>;
