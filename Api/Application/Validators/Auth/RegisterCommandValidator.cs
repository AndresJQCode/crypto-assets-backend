using Api.Application.Commands.AuthCommands;
using FluentValidation;

namespace Api.Application.Validators.Auth;

internal sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre es requerido")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder los 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El email es requerido")
            .EmailAddress()
            .WithMessage("El formato del email no es válido")
            .MaximumLength(256)
            .WithMessage("El email no puede exceder los 256 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("La contraseña es requerida")
            .MinimumLength(6)
            .WithMessage("La contraseña debe tener al menos 6 caracteres")
            .MaximumLength(100)
            .WithMessage("La contraseña no puede exceder los 100 caracteres")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?])")
            .WithMessage("La contraseña debe contener al menos una letra minúscula, una mayúscula, un número y un carácter especial");

        RuleFor(x => x.WhatsAppNumber)
            .NotEmpty()
            .WithMessage("El número de WhatsApp es requerido")
            .MaximumLength(20)
            .WithMessage("El número de WhatsApp no puede exceder los 20 caracteres")
            .Matches(@"^\+?[0-9\s\-]{10,20}$")
            .WithMessage("El número de WhatsApp debe contener solo dígitos, opcionalmente con + al inicio (ej. +573001234567)");

        RuleFor(x => x.TenantName)
            .NotEmpty()
            .WithMessage("El nombre del tenant (empresa) es requerido")
            .MaximumLength(200)
            .WithMessage("El nombre del tenant no puede exceder los 200 caracteres");
    }
}
