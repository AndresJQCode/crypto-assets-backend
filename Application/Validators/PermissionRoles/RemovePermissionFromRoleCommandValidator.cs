using Application.Commands.PermissionRoleCommands;
using FluentValidation;

namespace Application.Validators.PermissionRoles;

internal sealed class RemovePermissionFromRoleCommandValidator : AbstractValidator<RemovePermissionFromRoleCommand>
{
    public RemovePermissionFromRoleCommandValidator()
    {
        RuleFor(x => x.PermissionId)
            .NotEmpty()
            .WithMessage("El ID del permiso es requerido")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del permiso no puede ser vacío");

        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("El ID del rol es requerido")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del rol no puede ser vacío");
    }
}
