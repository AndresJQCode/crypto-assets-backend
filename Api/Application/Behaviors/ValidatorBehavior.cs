using Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Api.Application.Behaviors;

internal sealed class ValidatorBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        List<FluentValidation.Results.ValidationFailure>? failures = [.. validators
            .Select(v => v.Validate(request))
            .SelectMany(result => result.Errors)
            .Where(error => error != null)];

        if (failures.Count != 0)
        {
            string? msgToReturn = string.Join(", ", failures.Select(f => f.ErrorMessage));
            throw new DomainException(msgToReturn, new ValidationException("Validation exception", failures));
        }

        return await next(cancellationToken);
    }
}
