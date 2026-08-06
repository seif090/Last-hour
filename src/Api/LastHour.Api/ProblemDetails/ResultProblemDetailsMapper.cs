namespace LastHour.Api.ProblemDetails;

using LastHour.BuildingBlocks.SharedKernel.Results;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

/// <summary>
/// Maps a SharedKernel <see cref="Result"/> onto the HTTP surface. Failed results become
/// RFC 7807 <see cref="ProblemDetails"/> responses; successful results are passed through
/// so the value an endpoint returned is what gets serialized.
/// </summary>
public static class ResultProblemDetailsMapper
{
    /// <summary>
    /// Maps a failed result onto a <see cref="ProblemDetails"/> payload describing its first error.
    /// </summary>
    /// <param name="result">The failed result to map.</param>
    /// <returns>The RFC 7807 payload describing the failure.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> is successful.</exception>
    public static ProblemDetails Map(Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            throw new InvalidOperationException("A successful result cannot be mapped to problem details.");
        }

        return ErrorToProblemDetailsMapper.Map(result.FirstError ?? Error.Failure("UnknownError", "An unknown error occurred."));
    }

    /// <summary>
    /// Converts a <see cref="Result"/> returned by an endpoint into the object the response should carry:
    /// a problem details response for a failure, the unwrapped value for a successful generic result,
    /// or an empty success response for a successful non-generic result. Values that are not results
    /// are returned unchanged.
    /// </summary>
    /// <param name="value">The value an endpoint returned.</param>
    /// <param name="mapped">The response object when the value was a result.</param>
    /// <returns><see langword="true"/> when the value was a result; otherwise <see langword="false"/>.</returns>
    public static bool TryMap(object? value, out object? mapped)
    {
        switch (value)
        {
            case Result { IsFailure: true } failure:
                mapped = Results.Problem(Map(failure));
                return true;
            case Result success:
                mapped = Unwrap(success) ?? Results.Ok();
                return true;
            default:
                mapped = null;
                return false;
        }
    }

    private static object? Unwrap(Result result)
    {
        Type type = result.GetType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
        {
            return type.GetProperty("Value")?.GetValue(result);
        }

        return null;
    }
}
