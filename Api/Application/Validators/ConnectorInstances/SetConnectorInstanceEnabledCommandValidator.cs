using Api.Application.Commands.ConnectorInstanceCommands;
using FluentValidation;

namespace Api.Application.Validators.ConnectorInstances;

internal sealed class SetConnectorInstanceEnabledCommandValidator : AbstractValidator<SetConnectorInstanceEnabledCommand>
{
    public SetConnectorInstanceEnabledCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
