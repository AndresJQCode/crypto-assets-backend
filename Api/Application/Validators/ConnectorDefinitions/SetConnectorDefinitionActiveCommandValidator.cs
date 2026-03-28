using Api.Application.Commands.ConnectorDefinitionCommands;
using FluentValidation;

namespace Api.Application.Validators.ConnectorDefinitions;

internal sealed class SetConnectorDefinitionActiveCommandValidator : AbstractValidator<SetConnectorDefinitionActiveCommand>
{
    public SetConnectorDefinitionActiveCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
