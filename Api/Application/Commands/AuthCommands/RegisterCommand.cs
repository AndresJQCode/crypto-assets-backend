using Api.Application.Dtos.Auth;
using MediatR;

namespace Api.Application.Commands.AuthCommands;

internal sealed class RegisterCommand : IRequest<LoginResponseDto>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>Número de WhatsApp (ej. +573001234567). Requerido en el registro.</summary>
    public string WhatsAppNumber { get; set; } = string.Empty;
    /// <summary>Nombre del tenant (empresa/organización) que se crea en el registro. El slug se genera reemplazando espacios y caracteres especiales por guiones.</summary>
    public string TenantName { get; set; } = string.Empty;
    public string? RecaptchaToken { get; set; }
}
