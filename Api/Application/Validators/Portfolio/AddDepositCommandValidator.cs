using Api.Application.Commands.Portfolio;
using FluentValidation;

namespace Api.Application.Validators.Portfolio;

public class AddDepositCommandValidator : AbstractValidator<AddDepositCommand>
{
    public AddDepositCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Deposit amount must be positive");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes must not exceed 500 characters");
    }
}
