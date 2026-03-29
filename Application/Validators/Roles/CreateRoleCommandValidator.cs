using Application.Commands.RoleCommands;
using FluentValidation;

namespace Application.Validators.Roles;

internal sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del rol es requerido")
            .MaximumLength(256).WithMessage("El nombre del rol no puede exceder 256 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción del rol no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.PermissionIds)
            .NotEmpty()
            .WithMessage("Debe proporcionar al menos un permiso")
            .Must(permissionIds => permissionIds != null && permissionIds.All(id => Guid.TryParse(id, out var guid) && guid != Guid.Empty))
            .WithMessage("Todos los IDs de permisos deben ser GUIDs válidos")
            .Must(permissionIds => permissionIds != null && permissionIds.Count == permissionIds.Distinct().Count())
            .WithMessage("No se permiten permisos duplicados");
    }
}
