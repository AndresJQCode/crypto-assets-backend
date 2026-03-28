using Api.Application.Dtos.User;
using MediatR;

namespace Api.Application.Commands.UserCommands;

internal sealed record UpdateUserCommand(Guid Id, string Name, string Email, IReadOnlyCollection<string>? RoleIds) : IRequest<UserResponseDto>;
