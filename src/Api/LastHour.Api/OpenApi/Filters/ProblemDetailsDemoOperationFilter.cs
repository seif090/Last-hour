using System.Globalization;
using LastHour.Api.OpenApi.Examples;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LastHour.Api.OpenApi.Filters;

using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

/// <summary>
/// Documents the RFC 7807 problem details contract for the problem-details demonstration
/// endpoints: every relevant error status is declared with an application/problem+json payload
/// and an example, and the route parameter gets an example value so the Swagger UI can exercise
/// the endpoint immediately.
/// </summary>
public sealed class ProblemDetailsDemoOperationFilter : IOperationFilter
{
    private static readonly int[] ProblemStatusCodes =
    {
        StatusCodes.Status400BadRequest,
        StatusCodes.Status401Unauthorized,
        StatusCodes.Status403Forbidden,
        StatusCodes.Status404NotFound,
        StatusCodes.Status409Conflict,
        StatusCodes.Status500InternalServerError,
    };

    /// <summary>
    /// Adds the problem details error responses and the route parameter example to the demo operations.
    /// </summary>
    /// <param name="operation">The OpenAPI operation to update.</param>
    /// <param name="context">The operation filter context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        if (!IsProblemDetailsDemo(context.ApiDescription))
        {
            return;
        }

        OpenApiSchema schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository);
        operation.Responses.Remove("200");

        foreach (int statusCode in ProblemStatusCodes)
        {
            operation.Responses.TryAdd(
                statusCode.ToString(CultureInfo.InvariantCulture),
                new OpenApiResponse
                {
                    Description = DescriptionFor(statusCode),
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/problem+json"] = new OpenApiMediaType
                        {
                            Schema = schema,
                            Example = ProblemDetailsExampleFactory.Create(statusCode),
                        },
                    },
                });
        }

        foreach (OpenApiParameter parameter in operation.Parameters ?? Enumerable.Empty<OpenApiParameter>())
        {
            if (parameter.Name == "kind")
            {
                parameter.Description = "The error category to demonstrate: validation, not-found, " +
                                        "conflict, unauthorized, forbidden or unhandled.";
                parameter.Example = new OpenApiString("validation");
            }
        }
    }

    private static bool IsProblemDetailsDemo(ApiDescription apiDescription) =>
        apiDescription.RelativePath?.Contains("/system/problems/", StringComparison.OrdinalIgnoreCase) == true;

    private static string DescriptionFor(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Validation failed; see the errors extension for field-level details.",
        StatusCodes.Status401Unauthorized => "Authentication is required; supply a valid bearer token.",
        StatusCodes.Status403Forbidden => "The caller is authenticated but is not permitted to perform the request.",
        StatusCodes.Status404NotFound => "The requested resource could not be found.",
        StatusCodes.Status409Conflict => "The request conflicts with the current state of the resource.",
        _ => "An unexpected server error occurred; quote the correlation id when reporting it.",
    };
}
