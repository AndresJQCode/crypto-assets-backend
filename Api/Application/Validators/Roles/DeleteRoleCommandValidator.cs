using Api.Application.Commands.RoleCommands;
using FluentValidation;

namespace Api.Application.Validators.Roles;

internal sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del rol es requerido")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del rol no puede ser vacío");
    }
}
