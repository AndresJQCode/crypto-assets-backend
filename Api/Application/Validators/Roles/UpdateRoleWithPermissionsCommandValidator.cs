using Api.Application.Commands.RoleCommands;
using FluentValidation;

namespace Api.Application.Validators.Roles;

internal sealed class UpdateRoleWithPermissionsCommandValidator : AbstractValidator<UpdateRoleWithPermissionsCommand>
{
    public UpdateRoleWithPermissionsCommandValidator()
    {
        RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("El ID del rol es requerido")
                .Must(id => Guid.TryParse(id.ToString(), out var guid) && guid != Guid.Empty)
                .WithMessage("El ID del rol debe ser un GUID válido");

        RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("El nombre del rol es requerido")
                .MaximumLength(256)
                .WithMessage("El nombre del rol no puede exceder 256 caracteres");

        RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("La descripción del rol no puede exceder 500 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.PermissionIds)
                .NotNull()
                .WithMessage("La lista de permisos no puede ser nula")
                .Must(permissionIds => permissionIds == null || permissionIds.All(id => id != Guid.Empty))
                .WithMessage("Todos los IDs de permisos deben ser GUIDs válidos")
                .When(x => x.PermissionIds != null);
    }
}
