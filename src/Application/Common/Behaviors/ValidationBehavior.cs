using FluentValidation;
using MediatR;

namespace Application.Common.Behaviors;

/// <summary>
/// Validates incoming requests using all registered <see cref="IValidator{T}"/> instances for
/// <typeparamref name="TRequest"/> before invoking the handler.
/// </summary>
/// <typeparam name="TRequest">The type of the request being validated.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="validators">The validators registered for <typeparamref name="TRequest"/>.</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        this.validators = validators;
    }

    /// <inheritdoc />
    /// <exception cref="ValidationException">Thrown when one or more validators report failures.</exception>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)))
            .ConfigureAwait(false);

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next(cancellationToken).ConfigureAwait(false);
    }
}
