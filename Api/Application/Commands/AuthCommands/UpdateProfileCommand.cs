using Api.Application.Dtos.Auth;
using MediatR;

namespace Api.Application.Commands.AuthCommands;

internal sealed record UpdateProfileCommand(string Name) : IRequest<AuthUserDto>;
