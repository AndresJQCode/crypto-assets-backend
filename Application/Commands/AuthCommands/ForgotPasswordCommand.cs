using Application.Dtos.Auth;
using MediatR;

namespace Application.Commands.AuthCommands;

internal sealed class ForgotPasswordCommand : IRequest<ForgotPasswordResponseDto>
{
    public required string Email { get; set; }
    public string? RecaptchaToken { get; set; }
}
