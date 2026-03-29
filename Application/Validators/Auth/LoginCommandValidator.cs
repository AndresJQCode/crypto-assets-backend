using Application.Commands.AuthCommands;
using FluentValidation;

namespace Application.Validators.Auth;

internal sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
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
            .WithMessage("La contraseña no puede exceder los 100 caracteres");
    }
}

