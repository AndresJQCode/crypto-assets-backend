using Api.Application.Dtos.Auth;
using MediatR;

namespace Api.Application.Commands.AuthCommands;

internal sealed class ForgotPasswordCommand : IRequest<ForgotPasswordResponseDto>
{
    public required string Email { get; set; }
    public string? RecaptchaToken { get; set; }
}
