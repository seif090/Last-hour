using FluentValidation;
using FluentValidation.Results;
using LastHour.BuildingBlocks.Infrastructure.Results;
using LastHour.BuildingBlocks.SharedKernel.Results;
using MediatR;

namespace LastHour.BuildingBlocks.Infrastructure.Validation;

/// <summary>
/// Validates a request with all registered FluentValidation validators before it is handled,
/// and short-circuits the pipeline with a failed <see cref="Result"/> when validation fails.
/// </summary>
/// <typeparam name="TRequest">The type of the request to validate.</typeparam>
/// <typeparam name="TResponse">The result type returned by the request handler.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private static readonly Func<Error, TResponse> FailedResultFactory = ResultFailureFactory.Create<TResponse>();

    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="validators">The validators registered for <typeparamref name="TRequest"/>.</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// Validates the request with every registered validator and invokes the handler when validation succeeds.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <param name="next">The delegate that invokes the next pipeline step.</param>
    /// <param name="cancellationToken">The token used to signal cancellation.</param>
    /// <returns>A failed result containing all validation errors, or the handler result when validation succeeds.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        var failures = new List<ValidationFailure>();
        foreach (IValidator<TRequest> validator in _validators)
        {
            ValidationResult validationResult = await validator
                .ValidateAsync(request, cancellationToken)
                .ConfigureAwait(false);

            failures.AddRange(validationResult.Errors);
        }

        if (failures.Count == 0)
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        var validationError = new ValidationError(
            "ValidationFailed",
            "One or more validation errors occurred.",
            failures.Select(failure => Error.Validation(GetErrorCode(failure), failure.ErrorMessage)).ToArray());

        return FailedResultFactory(validationError);
    }

    /// <summary>
    /// Resolves the error code for a validation failure, falling back to the property name.
    /// </summary>
    /// <param name="failure">The validation failure.</param>
    /// <returns>The resolved error code.</returns>
    private static string GetErrorCode(ValidationFailure failure)
    {
        string code = string.IsNullOrWhiteSpace(failure.ErrorCode) ? failure.PropertyName : failure.ErrorCode;
        return string.IsNullOrWhiteSpace(code) ? "Validation" : code;
    }
}
