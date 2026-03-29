using Application.Dtos.Auth;
using MediatR;

namespace Application.Commands.AuthCommands;

internal sealed class RefreshTokenCommand : IRequest<LoginResponseDto>
{
    public required string RefreshToken { get; set; }
}
