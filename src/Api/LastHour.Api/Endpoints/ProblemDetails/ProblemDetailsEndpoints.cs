using LastHour.Api.ProblemDetails;
using LastHour.Api.Versioning;
using LastHour.BuildingBlocks.SharedKernel.Results;

namespace LastHour.Api.Endpoints.ProblemDetails;

/// <summary>
/// Maps the problem details demonstration endpoints: one route per supported error category
/// that returns a failed <see cref="Result"/>, and one that raises an unhandled exception.
/// Together they document the RFC 7807 error contract the API produces. The failed results
/// are converted into responses automatically by the <see cref="ResultEndpointFilter"/>.
/// </summary>
public static class ProblemDetailsEndpoints
{
    /// <summary>
    /// Maps the problem details demonstration endpoints onto the route table.
    /// </summary>
    /// <param name="app">The endpoint route builder to extend.</param>
    /// <returns>The same endpoint route builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
    public static IEndpointRouteBuilder MapProblemDetailsEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.NewVersionedApi("ProblemDetails")
            .MapGet("api/v{version:apiVersion}/system/problems/{kind}", (string kind) =>
            {
                Error[] validationErrors = new[]
                {
                    Error.Validation("Email", "'Email' is required."),
                    Error.Validation("Password", "'Password' must be at least 8 characters."),
                };

                Result result = kind.ToLowerInvariant() switch
                {
                    "validation" => Result.Failure(new ValidationError("ValidationFailed", "One or more validation errors occurred.", validationErrors)),
                    "not-found" => Result.Failure(Error.NotFound("ResourceNotFound", "The requested resource could not be found.")),
                    "conflict" => Result.Failure(Error.Conflict("ResourceConflict", "The operation conflicts with the current state of the resource.")),
                    "unauthorized" => Result.Failure(Error.Unauthorized("Unauthorized", "Authentication is required to access this resource.")),
                    "forbidden" => Result.Failure(Error.Forbidden("Forbidden", "You do not have permission to access this resource.")),
                    "unhandled" => throw new InvalidOperationException("A demonstration unhandled exception."),
                    _ => Result.Failure(Error.Failure("UnknownProblemKind", $"The problem kind '{kind}' is not recognized.")),
                };

                return result;
            })
            .HasApiVersion(ApiVersions.V1)
            .WithName("ProblemDetailsDemo")
            .WithTags("ProblemDetails")
            .MapResultToProblemDetails();

        return app;
    }
}
