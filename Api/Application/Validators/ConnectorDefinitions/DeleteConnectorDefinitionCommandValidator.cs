using Api.Application.Commands.ConnectorDefinitionCommands;
using FluentValidation;

namespace Api.Application.Validators.ConnectorDefinitions;

internal sealed class DeleteConnectorDefinitionCommandValidator : AbstractValidator<DeleteConnectorDefinitionCommand>
{
    public DeleteConnectorDefinitionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
