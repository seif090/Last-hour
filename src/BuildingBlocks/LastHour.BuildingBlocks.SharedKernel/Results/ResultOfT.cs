using System.Diagnostics.CodeAnalysis;

namespace LastHour.BuildingBlocks.SharedKernel.Results;

/// <summary>
/// Represents the outcome of an operation that produces a value, distinguishing between
/// success and one or more expected, non-exceptional failures.
/// </summary>
/// <typeparam name="TValue">The type of the value produced on success.</typeparam>
[SuppressMessage(
    "Design",
    "CA1000",
    Justification = "Static factories on Result<T> are the core API of the functional result pattern.")]
public class Result<TValue> : Result
{
    private readonly TValue _value;

    private Result(TValue value)
        : base(Array.Empty<Error>())
    {
        _value = value;
    }

    private Result(Error[] errors)
        : base(errors)
    {
        _value = default!;
    }

    /// <summary>
    /// Gets the value of a successful result.
    /// </summary>
    /// <exception cref="InvalidOperationException">The result failed.</exception>
    public TValue Value => IsSuccess ? _value : throw new InvalidOperationException("Cannot access the value of a failed result.");

    /// <summary>
    /// Gets the value of a successful result, or the default value when the result failed.
    /// </summary>
    public TValue ValueOrDefault => IsSuccess ? _value : default!;

    /// <summary>
    /// Implicitly converts a value into a successful <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="value">The value produced by the operation.</param>
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>
    /// Implicitly converts an <see cref="Error"/> into a failed <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="error">The error that caused the failure.</param>
    public static implicit operator Result<TValue>(Error error) => Failure(error);

    /// <summary>
    /// Creates a successful result with the given value.
    /// </summary>
    /// <param name="value">The value produced by the operation.</param>
    /// <returns>A successful <see cref="Result{TValue}"/>.</returns>
    public static Result<TValue> Success(TValue value) => new Result<TValue>(value);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">The error that caused the failure.</param>
    /// <returns>A failed <see cref="Result{TValue}"/>.</returns>
    public static new Result<TValue> Failure(Error error) => new Result<TValue>(new[] { error });

    /// <summary>
    /// Creates a failed result with one or more errors.
    /// </summary>
    /// <param name="errors">The errors that caused the failure.</param>
    /// <returns>A failed <see cref="Result{TValue}"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errors"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="errors"/> is empty.</exception>
    public static new Result<TValue> Failure(IEnumerable<Error> errors) => new Result<TValue>(ValidateErrors(errors));

    /// <summary>
    /// Creates a successful result from a value.
    /// </summary>
    /// <param name="value">The value produced by the operation.</param>
    /// <returns>A successful <see cref="Result{TValue}"/>.</returns>
    public static Result<TValue> FromValue(TValue value) => Success(value);

    /// <summary>
    /// Creates a failed result from an error.
    /// </summary>
    /// <param name="error">The error that caused the failure.</param>
    /// <returns>A failed <see cref="Result{TValue}"/>.</returns>
    public static new Result<TValue> FromError(Error error) => Failure(error);

    /// <summary>
    /// Tries to retrieve the value of a successful result.
    /// </summary>
    /// <param name="value">The value when the result is successful; otherwise the default value.</param>
    /// <returns><see langword="true"/> when the result is successful; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue(out TValue value)
    {
        value = _value;
        return IsSuccess;
    }

    /// <summary>
    /// Projects the value of a successful result, propagating failures unchanged.
    /// </summary>
    /// <typeparam name="TNext">The type of the projected value.</typeparam>
    /// <param name="mapper">The mapping function applied on success.</param>
    /// <returns>A successful <see cref="Result{TNext}"/> on success; otherwise a failed one.</returns>
    public Result<TNext> Map<TNext>(Func<TValue, TNext> mapper)
    {
        if (IsFailure)
        {
            return Result<TNext>.FromErrors(Errors);
        }

        return Result<TNext>.Success(mapper(_value));
    }

    /// <summary>
    /// Chains an operation that returns a <see cref="Result{TNext}"/>, propagating failures unchanged.
    /// </summary>
    /// <typeparam name="TNext">The type of the chained value.</typeparam>
    /// <param name="binder">The chained operation invoked on success.</param>
    /// <returns>The chained result on success; otherwise a failed <see cref="Result{TNext}"/>.</returns>
    public Result<TNext> Bind<TNext>(Func<TValue, Result<TNext>> binder)
    {
        if (IsFailure)
        {
            return Result<TNext>.FromErrors(Errors);
        }

        return binder(_value);
    }

    /// <summary>
    /// Ensures that a condition on the value holds, returning a failed result with <paramref name="error"/> otherwise.
    /// </summary>
    /// <param name="predicate">The condition to satisfy.</param>
    /// <param name="error">The error used when the condition is not satisfied.</param>
    /// <returns>The current result when the condition is satisfied; otherwise a failed result.</returns>
    public Result<TValue> Ensure(Func<TValue, bool> predicate, Error error)
    {
        if (IsSuccess && !predicate(_value))
        {
            return Failure(error);
        }

        return this;
    }

    /// <summary>
    /// Invokes an action with the value when the result is successful, returning the same result.
    /// </summary>
    /// <param name="action">The action to invoke on success.</param>
    /// <returns>The current result.</returns>
    public Result<TValue> Tap(Action<TValue> action)
    {
        if (IsSuccess)
        {
            action(_value);
        }

        return this;
    }

    /// <summary>
    /// Converts the result into a single value.
    /// </summary>
    /// <typeparam name="TResult">The type of the converted value.</typeparam>
    /// <param name="onSuccess">The function invoked with the value on success.</param>
    /// <param name="onFailure">The function invoked with the first error on failure.</param>
    /// <returns>The value produced by the matching function.</returns>
    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess(_value) : onFailure(FirstError!);

    /// <summary>
    /// Executes one of two actions depending on the outcome.
    /// </summary>
    /// <param name="onSuccess">The action invoked with the value on success.</param>
    /// <param name="onFailure">The action invoked with the first error on failure.</param>
    public void Match(Action<TValue> onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess)
        {
            onSuccess(_value);
        }
        else
        {
            onFailure(FirstError!);
        }
    }

    /// <summary>
    /// Creates a failed <see cref="Result{TValue}"/> from a collection of errors.
    /// </summary>
    /// <param name="errors">The errors that caused the failure.</param>
    /// <returns>A failed <see cref="Result{TValue}"/>.</returns>
    internal static Result<TValue> FromErrors(IReadOnlyList<Error> errors) => new Result<TValue>(errors.ToArray());
}
