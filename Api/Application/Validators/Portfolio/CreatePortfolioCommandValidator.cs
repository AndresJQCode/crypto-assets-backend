using Api.Application.Commands.Portfolio;
using FluentValidation;

namespace Api.Application.Validators.Portfolio;

public class CreatePortfolioCommandValidator : AbstractValidator<CreatePortfolioCommand>
{
    public CreatePortfolioCommandValidator()
    {
        RuleFor(x => x.InitialCapital)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Initial capital cannot be negative");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .MaximumLength(10)
            .WithMessage("Currency must not exceed 10 characters");
    }
}
