using Api.Application.Commands.AuthCommands;
using FluentValidation;

namespace Api.Application.Validators.Auth;

internal sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("El refresh token es requerido")
            .MaximumLength(500)
            .WithMessage("El refresh token no puede exceder los 500 caracteres");
    }
}
