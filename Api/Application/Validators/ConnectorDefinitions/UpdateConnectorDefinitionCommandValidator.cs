using Api.Application.Commands.ConnectorDefinitionCommands;
using FluentValidation;

namespace Api.Application.Validators.ConnectorDefinitions;

internal sealed class UpdateConnectorDefinitionCommandValidator : AbstractValidator<UpdateConnectorDefinitionCommand>
{
    public UpdateConnectorDefinitionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LogoUrl).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.LogoUrl));
        RuleFor(x => x.Description).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Description));
    }
}
