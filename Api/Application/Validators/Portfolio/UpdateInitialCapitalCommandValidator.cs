using Api.Application.Commands.Portfolio;
using FluentValidation;

namespace Api.Application.Validators.Portfolio;

public class UpdateInitialCapitalCommandValidator : AbstractValidator<UpdateInitialCapitalCommand>
{
    public UpdateInitialCapitalCommandValidator()
    {
        RuleFor(x => x.NewInitialCapital)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Initial capital cannot be negative");
    }
}
