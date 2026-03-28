using Api.Application.Commands.UserCommands;
using FluentValidation;

namespace Api.Application.Validators.Users;

internal sealed class UpdateUserStatusCommandValidator : AbstractValidator<UpdateUserStatusCommand>
{
    public UpdateUserStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido")
            .Must(id => Guid.TryParse(id.ToString(), out var guid) && guid != Guid.Empty)
            .WithMessage("El ID del usuario debe ser un GUID válido");
    }
}
