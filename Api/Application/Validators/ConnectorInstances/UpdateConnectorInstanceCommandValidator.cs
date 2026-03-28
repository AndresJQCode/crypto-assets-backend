using Api.Application.Commands.ConnectorInstanceCommands;
using FluentValidation;

namespace Api.Application.Validators.ConnectorInstances;

internal sealed class UpdateConnectorInstanceCommandValidator : AbstractValidator<UpdateConnectorInstanceCommand>
{
    public UpdateConnectorInstanceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
