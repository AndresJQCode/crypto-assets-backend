using Api.Application.Commands.AuthCommands;
using FluentValidation;

namespace Api.Application.Validators.Auth;

internal sealed class ExchangeCodeCommandValidator : AbstractValidator<ExchangeCodeCommand>
{
    public ExchangeCodeCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("El código de autorización es requerido")
            .MaximumLength(10000)
            .WithMessage("El código no puede exceder los 10000 caracteres");

        RuleFor(x => x.Provider)
            .NotEmpty()
            .WithMessage("El proveedor es requerido")
            .Must(BeValidProvider)
            .WithMessage("El proveedor debe ser 'Google' o 'Microsoft'");
    }

    private static bool BeValidProvider(string provider)
    {
        return provider?.ToUpperInvariant() switch
        {
            "GOOGLE" or "MICROSOFT" => true,
            _ => false
        };
    }
}

