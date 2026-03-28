using Api.Application.Commands.ConnectorInstanceCommands;
using FluentValidation;

namespace Api.Application.Validators.ConnectorInstances;

internal sealed class DeleteConnectorInstanceCommandValidator : AbstractValidator<DeleteConnectorInstanceCommand>
{
    public DeleteConnectorInstanceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
