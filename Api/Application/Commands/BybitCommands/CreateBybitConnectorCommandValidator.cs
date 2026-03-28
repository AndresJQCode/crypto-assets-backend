using FluentValidation;

namespace Api.Application.Commands.BybitCommands;

internal sealed class CreateBybitConnectorCommandValidator : AbstractValidator<CreateBybitConnectorCommand>
{
    public CreateBybitConnectorCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del conector es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.ApiKey)
            .NotEmpty().WithMessage("La API key de Bybit es requerida")
            .MaximumLength(500).WithMessage("La API key no puede exceder 500 caracteres");

        RuleFor(x => x.ApiSecret)
            .NotEmpty().WithMessage("El API secret de Bybit es requerido")
            .MaximumLength(500).WithMessage("El API secret no puede exceder 500 caracteres");
    }
}
