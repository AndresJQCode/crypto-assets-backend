using Api.Application.Dtos.Auth;
using MediatR;

namespace Api.Application.Commands.AuthCommands;

internal sealed class LoginCommand : IRequest<LoginResponseDto>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? RecaptchaToken { get; set; }
}
