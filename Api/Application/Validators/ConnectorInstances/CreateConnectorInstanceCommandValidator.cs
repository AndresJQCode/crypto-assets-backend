using Api.Application.Commands.ConnectorInstanceCommands;
using FluentValidation;

namespace Api.Application.Validators.ConnectorInstances;

internal sealed class CreateConnectorInstanceCommandValidator : AbstractValidator<CreateConnectorInstanceCommand>
{
    public CreateConnectorInstanceCommandValidator()
    {
        RuleFor(x => x.ConnectorDefinitionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ConfigurationJson).NotEmpty();
        RuleFor(x => x.AccessToken).NotEmpty();
    }
}
