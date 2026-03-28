using Api.Application.Commands.UserCommands;
using FluentValidation;

namespace Api.Application.Validators.Users;

internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
                .Must(id => Guid.TryParse(id.ToString(), out var guid) && guid != Guid.Empty)
                .WithMessage("El ID del usuario debe ser un GUID válido");

        RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("El nombre es requerido")
                .MinimumLength(2)
                .WithMessage("El nombre debe tener al menos 2 caracteres")
                .MaximumLength(100)
                .WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("El email es requerido")
                .EmailAddress()
                .WithMessage("El email debe tener un formato válido")
                .MaximumLength(256)
                .WithMessage("El email no puede exceder 256 caracteres");

        RuleFor(x => x.RoleIds)
                .Must(roleIds => roleIds != null && roleIds.Count > 0 && roleIds.All(id => Guid.TryParse(id.ToString(), out var guid) && guid != Guid.Empty))
                .WithMessage("Todos los IDs de roles deben ser GUID válidos")
                .Must(roleIds => roleIds == null || roleIds.Count == roleIds.Distinct().Count())
                .WithMessage("No se permiten roles duplicados");
    }
}
