using Api.Application.Commands.UserCommands;
using FluentValidation;

namespace Api.Application.Validators.Users;

internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El email es requerido")
            .EmailAddress()
            .WithMessage("El email debe tener un formato válido");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre es requerido")
            .MinimumLength(2)
            .WithMessage("El nombre debe tener al menos 2 caracteres")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Roles)
            .NotEmpty()
            .WithMessage("Los roles son requeridos")
            .Must(roles => roles != null && roles.All(role => Guid.TryParse(role, out var guid) && guid != Guid.Empty))
            .WithMessage("Todos los roles deben ser GUIDs válidos")
            .Must(roles => roles != null && roles.Count == roles.Distinct().Count())
            .WithMessage("No se permiten roles duplicados");
    }
}
