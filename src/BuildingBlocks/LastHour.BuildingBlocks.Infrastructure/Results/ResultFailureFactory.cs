using System.Reflection;
using LastHour.BuildingBlocks.SharedKernel.Results;

namespace LastHour.BuildingBlocks.Infrastructure.Results;

/// <summary>
/// Creates factories that convert an <see cref="Error"/> into a failed result of a given type.
/// </summary>
internal static class ResultFailureFactory
{
    /// <summary>
    /// Creates a factory that returns a failed <typeparamref name="TResponse"/> carrying the given error.
    /// </summary>
    /// <typeparam name="TResponse">The result type to produce.</typeparam>
    /// <returns>A factory that creates a failed result from an error.</returns>
    /// <exception cref="InvalidOperationException"><typeparamref name="TResponse"/> is not a supported result type.</exception>
    internal static Func<Error, TResponse> Create<TResponse>()
        where TResponse : Result
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return error => (TResponse)(object)Result.Failure(error);
        }

        if (!typeof(TResponse).IsGenericType || typeof(TResponse).GetGenericTypeDefinition() != typeof(Result<>))
        {
            throw new InvalidOperationException(
                $"The result failure factory does not support the response type '{typeof(TResponse)}'.");
        }

        Type valueType = typeof(TResponse).GetGenericArguments()[0];
        MethodInfo failureMethod = typeof(Result<>)
            .MakeGenericType(valueType)
            .GetMethod(nameof(Result.Failure), BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Error) }, null)
            ?? throw new InvalidOperationException($"Result<{valueType.Name}> does not expose a Failure(Error) factory.");

        return error =>
        {
            object? failedResult = failureMethod.Invoke(null, new object[] { error });
            return (TResponse)failedResult!;
        };
    }
}
