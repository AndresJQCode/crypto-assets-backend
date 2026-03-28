using Api.Application.Dtos.User;
using MediatR;

namespace Api.Application.Commands.UserCommands;

internal sealed record CreateUserCommand(string Email, string Name, IReadOnlyCollection<string> Roles) : IRequest<UserResponseDto>;
