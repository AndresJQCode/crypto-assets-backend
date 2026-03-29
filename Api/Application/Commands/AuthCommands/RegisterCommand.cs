using Api.Application.Dtos.Auth;
using MediatR;

namespace Api.Application.Commands.AuthCommands;

internal sealed class RegisterCommand : IRequest<LoginResponseDto>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? RecaptchaToken { get; set; }
}
