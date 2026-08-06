using Microsoft.OpenApi.Any;

namespace LastHour.Api.OpenApi.Examples;

/// <summary>
/// Builds the example payloads that document the RFC 7807 problem details contract in Swagger:
/// each error status gets a realistic application/problem+json body in the exact wire format the
/// API emits.
/// </summary>
public static class ProblemDetailsExampleFactory
{
    private const string RequestInstance = "urn:lasthour:request:00000000-0000-0000-0000-000000000001";

    /// <summary>
    /// Gets the validation problem details example used for the shared ProblemDetails schema.
    /// </summary>
    public static IOpenApiAny Validation => ToOpenApiAny(ValidationExample);

    private static object ValidationExample => new
    {
        type = "https://lasthour.dev/errors/validationerror",
        title = "One or more validation errors occurred.",
        status = 400,
        detail = "See the errors extension for details.",
        instance = RequestInstance,
        code = "ValidationFailed",
        errors = new { Email = new[] { "'Email' is required." } },
    };

    /// <summary>
    /// Creates the problem details example for the given HTTP status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code the example applies to.</param>
    /// <returns>The OpenAPI example payload.</returns>
    public static IOpenApiAny Create(int statusCode) => ToOpenApiAny(ExampleFor(statusCode));

    private static object ExampleFor(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => ValidationExample,
        StatusCodes.Status401Unauthorized => new
        {
            type = "https://lasthour.dev/errors/unauthorized",
            title = "Unauthorized",
            status = 401,
            detail = "Authentication is required to access this resource.",
            instance = RequestInstance,
            code = "Unauthorized",
        },
        StatusCodes.Status403Forbidden => new
        {
            type = "https://lasthour.dev/errors/forbidden",
            title = "Forbidden",
            status = 403,
            detail = "You do not have permission to access this resource.",
            instance = RequestInstance,
            code = "Forbidden",
        },
        StatusCodes.Status404NotFound => new
        {
            type = "https://lasthour.dev/errors/notfound",
            title = "Not Found",
            status = 404,
            detail = "The requested resource could not be found.",
            instance = RequestInstance,
            code = "ResourceNotFound",
        },
        StatusCodes.Status409Conflict => new
        {
            type = "https://lasthour.dev/errors/conflict",
            title = "Conflict",
            status = 409,
            detail = "The operation conflicts with the current state of the resource.",
            instance = RequestInstance,
            code = "ResourceConflict",
        },
        _ => new
        {
            type = "https://lasthour.dev/errors/failure",
            title = "An unexpected error occurred.",
            status = 500,
            detail = "An unexpected error occurred while processing your request.",
            instance = RequestInstance,
            code = "UnhandledException",
        },
    };

    private static IOpenApiAny ToOpenApiAny(object value) => OpenApiExampleConverter.ToOpenApiAny(value);
}
