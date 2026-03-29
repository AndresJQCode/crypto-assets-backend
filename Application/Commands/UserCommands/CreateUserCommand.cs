using Application.Dtos.User;
using MediatR;

namespace Application.Commands.UserCommands;

internal sealed record CreateUserCommand(string Email, string Name, IReadOnlyCollection<string> Roles) : IRequest<UserResponseDto>;
