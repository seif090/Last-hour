using System.Collections.ObjectModel;

namespace LastHour.BuildingBlocks.SharedKernel.Results;

/// <summary>
/// Represents the outcome of an operation that does not produce a value, distinguishing
/// between success and one or more expected, non-exceptional failures.
/// </summary>
public class Result
{
    private readonly ReadOnlyCollection<Error> _errors;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="errors">The errors associated with a failed operation; empty for a successful operation.</param>
    protected Result(Error[] errors)
    {
        _errors = Array.AsReadOnly(errors);
        IsSuccess = errors.Length == 0;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the errors associated with a failed operation; empty for a successful operation.
    /// </summary>
    public ReadOnlyCollection<Error> Errors => _errors;

    /// <summary>
    /// Gets the first error of a failed operation, or <see langword="null"/> when the operation succeeded.
    /// </summary>
    public Error? FirstError => _errors.Count == 0 ? null : _errors[0];

    /// <summary>
    /// Gets the <see cref="ErrorType"/> of the first error, or <see cref="ErrorType.None"/> when the operation succeeded.
    /// </summary>
    public ErrorType ErrorType => FirstError?.Type ?? ErrorType.None;

    /// <summary>
    /// Implicitly converts an <see cref="Error"/> into a failed <see cref="Result"/>.
    /// </summary>
    /// <param name="error">The error that caused the failure.</param>
    public static implicit operator Result(Error error) => Failure(error);

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Success() => new Result(Array.Empty<Error>());

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">The error that caused the failure.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(Error error) => new Result(new[] { error });

    /// <summary>
    /// Creates a failed result with one or more errors.
    /// </summary>
    /// <param name="errors">The errors that caused the failure.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errors"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="errors"/> is empty.</exception>
    public static Result Failure(IEnumerable<Error> errors) => new Result(ValidateErrors(errors));

    /// <summary>
    /// Creates a failed result from an error.
    /// </summary>
    /// <param name="error">The error that caused the failure.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result FromError(Error error) => Failure(error);

    /// <summary>
    /// Ensures that a condition holds, returning a failed result with <paramref name="error"/> otherwise.
    /// </summary>
    /// <param name="predicate">The condition to satisfy.</param>
    /// <param name="error">The error used when the condition is not satisfied.</param>
    /// <returns>The current result when successful and the condition is satisfied; otherwise a failed result.</returns>
    public Result Ensure(Func<bool> predicate, Error error)
    {
        if (IsSuccess && !predicate())
        {
            return Failure(error);
        }

        return this;
    }

    /// <summary>
    /// Projects a successful result to a new value, propagating failures unchanged.
    /// </summary>
    /// <typeparam name="TValue">The type of the projected value.</typeparam>
    /// <param name="mapper">The mapping function applied on success.</param>
    /// <returns>A successful <see cref="Result{TValue}"/> on success; otherwise a failed one.</returns>
    public Result<TValue> Map<TValue>(Func<TValue> mapper)
    {
        if (IsFailure)
        {
            return Result<TValue>.FromErrors(_errors);
        }

        return Result<TValue>.Success(mapper());
    }

    /// <summary>
    /// Chains an operation that returns a <see cref="Result{TValue}"/>, propagating failures unchanged.
    /// </summary>
    /// <typeparam name="TValue">The type of the chained value.</typeparam>
    /// <param name="binder">The chained operation invoked on success.</param>
    /// <returns>The chained result on success; otherwise a failed <see cref="Result{TValue}"/>.</returns>
    public Result<TValue> Bind<TValue>(Func<Result<TValue>> binder)
    {
        if (IsFailure)
        {
            return Result<TValue>.FromErrors(_errors);
        }

        return binder();
    }

    /// <summary>
    /// Invokes an action when the result is successful, returning the same result.
    /// </summary>
    /// <param name="action">The action to invoke on success.</param>
    /// <returns>The current result.</returns>
    public Result Tap(Action action)
    {
        if (IsSuccess)
        {
            action();
        }

        return this;
    }

    /// <summary>
    /// Converts the result into a single value.
    /// </summary>
    /// <typeparam name="TResult">The type of the converted value.</typeparam>
    /// <param name="onSuccess">The function invoked on success.</param>
    /// <param name="onFailure">The function invoked with the first error on failure.</param>
    /// <returns>The value produced by the matching function.</returns>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess() : onFailure(FirstError!);

    /// <summary>
    /// Executes one of two actions depending on the outcome.
    /// </summary>
    /// <param name="onSuccess">The action invoked on success.</param>
    /// <param name="onFailure">The action invoked with the first error on failure.</param>
    public void Match(Action onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess)
        {
            onSuccess();
        }
        else
        {
            onFailure(FirstError!);
        }
    }

    /// <summary>
    /// Validates and materializes a collection of errors.
    /// </summary>
    /// <param name="errors">The errors to validate.</param>
    /// <returns>The materialized errors.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errors"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="errors"/> is empty.</exception>
    protected static Error[] ValidateErrors(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        Error[] materialized = errors.ToArray();
        if (materialized.Length == 0)
        {
            throw new ArgumentException("A failed result requires at least one error.", nameof(errors));
        }

        return materialized;
    }
}
