using Application.Commands.AuthCommands;
using FluentValidation;

namespace Application.Validators.Auth;

internal sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido")
                .Length(2, 100).WithMessage("El nombre debe tener entre 2 y 100 caracteres")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$").WithMessage("El nombre solo puede contener letras y espacios");

    }
}
