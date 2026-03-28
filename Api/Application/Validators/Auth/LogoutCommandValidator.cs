using Api.Application.Commands.AuthCommands;
using FluentValidation;

namespace Api.Application.Validators.Auth;

internal sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido")
            .MaximumLength(450)
            .WithMessage("El ID del usuario no puede exceder los 450 caracteres");
    }
}
