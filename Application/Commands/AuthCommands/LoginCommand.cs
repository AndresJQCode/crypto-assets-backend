using Application.Dtos.Auth;
using MediatR;

namespace Application.Commands.AuthCommands;

internal sealed class LoginCommand : IRequest<LoginResponseDto>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? RecaptchaToken { get; set; }
}
