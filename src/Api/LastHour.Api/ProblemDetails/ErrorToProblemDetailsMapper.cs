namespace LastHour.Api.ProblemDetails;

using LastHour.BuildingBlocks.SharedKernel.Results;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

/// <summary>
/// Maps a SharedKernel <see cref="Error"/> onto an RFC 7807 <see cref="ProblemDetails"/>
/// payload. The mapping is the single place where error categories become HTTP semantics:
/// <see cref="ErrorType"/> drives the status code, title and type URI, while the error code
/// is always surfaced as a <c>code</c> extension and validation failures as an <c>errors</c>
/// dictionary.
/// </summary>
public static class ErrorToProblemDetailsMapper
{
    private const string ErrorTypeBaseUri = "https://lasthour.dev/errors/";

    /// <summary>
    /// Maps an error onto a <see cref="ProblemDetails"/> payload.
    /// </summary>
    /// <param name="error">The error to map.</param>
    /// <returns>The RFC 7807 payload describing the error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/>.</exception>
    public static ProblemDetails Map(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        var problemDetails = new ProblemDetails
        {
            Status = StatusFor(error.Type),
            Title = TitleFor(error.Type),
            Detail = error.Description,
            Type = TypeUriFor(error.Type),
        };

        problemDetails.Extensions["code"] = error.Code;

        if (error is ValidationError validationError && validationError.Validations.Count > 0)
        {
            problemDetails.Extensions["errors"] = validationError.Validations
                .GroupBy(validation => validation.Code, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(validation => validation.Description).ToArray(),
                    StringComparer.Ordinal);
        }

        return problemDetails;
    }

    /// <summary>
    /// Resolves the HTTP status code for an error category.
    /// </summary>
    /// <param name="type">The error category.</param>
    /// <returns>The HTTP status code that best represents the category.</returns>
    public static int StatusFor(ErrorType type)
        => type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

    /// <summary>
    /// Resolves the RFC 7807 title for an error category.
    /// </summary>
    /// <param name="type">The error category.</param>
    /// <returns>A short, stable title for the category.</returns>
    public static string TitleFor(ErrorType type)
        => type switch
        {
            ErrorType.Validation => "Bad Request",
            ErrorType.NotFound => "Not Found",
            ErrorType.Conflict => "Conflict",
            ErrorType.Unauthorized => "Unauthorized",
            ErrorType.Forbidden => "Forbidden",
            _ => "Internal Server Error",
        };

    /// <summary>
    /// Resolves the RFC 7807 type URI for an error category.
    /// </summary>
    /// <param name="type">The error category.</param>
    /// <returns>A stable URI identifying the error category.</returns>
    public static string TypeUriFor(ErrorType type) => ErrorTypeBaseUri + type.ToString().ToLowerInvariant();
}
