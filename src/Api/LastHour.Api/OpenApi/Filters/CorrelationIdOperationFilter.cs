using LastHour.Api.Middleware;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LastHour.Api.OpenApi.Filters;

/// <summary>
/// Documents the X-Correlation-ID header on every operation so callers can trace requests across
/// services. The header is optional: the correlation id middleware generates one when it is absent
/// and echoes the value back on the response.
/// </summary>
public sealed class CorrelationIdOperationFilter : IOperationFilter
{
    /// <summary>
    /// Adds the correlation id header parameter to the OpenAPI operation.
    /// </summary>
    /// <param name="operation">The OpenAPI operation to update.</param>
    /// <param name="context">The operation filter context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (operation.Parameters?.Any(parameter => parameter.Name == CorrelationIdDefaults.HeaderName) == true)
        {
            return;
        }

        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = CorrelationIdDefaults.HeaderName,
            In = ParameterLocation.Header,
            Required = false,
            Description = "Optional correlation id used to trace the request across services. " +
                          "A new value is generated when this header is omitted and echoed back on the response.",
            Example = new OpenApiString("0192e5f0-0000-7000-8000-000000000001"),
            Schema = new OpenApiSchema { Type = "string" },
        });
    }
}
