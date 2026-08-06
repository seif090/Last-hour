using LastHour.Api.OpenApi.Examples;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LastHour.Api.OpenApi.Filters;

/// <summary>
/// Adds realistic example values to the response schemas the LastHour endpoints return, so the
/// Swagger UI shows a sample payload next to each model.
/// </summary>
public sealed class EndpointExampleSchemaFilter : ISchemaFilter
{
    /// <summary>
    /// Assigns an example to the schemas it recognizes.
    /// </summary>
    /// <param name="schema">The OpenAPI schema to update.</param>
    /// <param name="context">The schema filter context.</param>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(context);

        if (context.Type is null || schema.Example is not null)
        {
            return;
        }

        schema.Example = context.Type.Name switch
        {
            "ServiceInfo" => FromJson(new { service = "LastHour.Api", status = "ready" }),
            "SystemStatus" => FromJson(new { version = "v1" }),
            "ProblemDetails" => ProblemDetailsExampleFactory.Validation,
            _ => schema.Example,
        };
    }

    private static IOpenApiAny FromJson(object value) => OpenApiExampleConverter.ToOpenApiAny(value);
}
