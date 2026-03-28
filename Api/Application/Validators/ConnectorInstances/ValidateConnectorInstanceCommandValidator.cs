using Api.Application.Commands.ConnectorInstanceCommands;
using FluentValidation;

namespace Api.Application.Validators.ConnectorInstances;

internal sealed class ValidateConnectorInstanceCommandValidator : AbstractValidator<ValidateConnectorInstanceCommand>
{
    public ValidateConnectorInstanceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
