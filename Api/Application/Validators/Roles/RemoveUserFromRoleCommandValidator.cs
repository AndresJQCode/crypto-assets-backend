using Api.Application.Commands.RoleCommands;
using FluentValidation;

namespace Api.Application.Validators.Roles;

internal sealed class RemoveUserFromRoleCommandValidator : AbstractValidator<RemoveUserFromRoleCommand>
{
    public RemoveUserFromRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido")
            .MaximumLength(450)
            .WithMessage("El ID del usuario no puede exceder los 450 caracteres");

        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("El ID del rol es requerido")
            .MaximumLength(450)
            .WithMessage("El ID del rol no puede exceder los 450 caracteres");
    }
}
