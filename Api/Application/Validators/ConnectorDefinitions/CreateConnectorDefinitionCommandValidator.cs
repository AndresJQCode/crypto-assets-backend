using Api.Application.Commands.ConnectorDefinitionCommands;
using FluentValidation;

namespace Api.Application.Validators.ConnectorDefinitions;

internal sealed class CreateConnectorDefinitionCommandValidator : AbstractValidator<CreateConnectorDefinitionCommand>
{
    public CreateConnectorDefinitionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ProviderType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CategoryType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LogoUrl).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.LogoUrl));
        RuleFor(x => x.Description).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Description));
    }
}
