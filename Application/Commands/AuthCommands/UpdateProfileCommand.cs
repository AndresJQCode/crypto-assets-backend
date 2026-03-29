using Application.Dtos.Auth;
using MediatR;

namespace Application.Commands.AuthCommands;

internal sealed record UpdateProfileCommand(string Name) : IRequest<AuthUserDto>;
