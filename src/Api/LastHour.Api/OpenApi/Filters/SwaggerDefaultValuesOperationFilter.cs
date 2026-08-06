using LastHour.Api.OpenApi.Examples;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LastHour.Api.OpenApi.Filters;

/// <summary>
/// Applies the default values, descriptions and required flags that the versioned API explorer
/// reports to the corresponding Swagger operation parameters (see the Asp.Versioning reference
/// implementation).
/// </summary>
public sealed class SwaggerDefaultValuesOperationFilter : IOperationFilter
{
    /// <summary>
    /// Copies parameter metadata from the API description onto the OpenAPI operation.
    /// </summary>
    /// <param name="operation">The OpenAPI operation to update.</param>
    /// <param name="context">The operation filter context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        ApiDescription apiDescription = context.ApiDescription;
        operation.Deprecated |= apiDescription.IsDeprecated();

        if (operation.Parameters is null)
        {
            return;
        }

        foreach (OpenApiParameter parameter in operation.Parameters)
        {
            ApiParameterDescription? parameterDescription = apiDescription.ParameterDescriptions
                .FirstOrDefault(description => description.Name == parameter.Name);

            if (parameterDescription is null || parameter.Schema is null)
            {
                continue;
            }

            if (parameter.Description is null)
            {
                parameter.Description = parameterDescription.ModelMetadata?.Description;
            }

            if (parameter.Schema.Default is null
                && parameterDescription.DefaultValue is not null
                && parameterDescription.ModelMetadata is not null)
            {
                parameter.Schema.Default = OpenApiExampleConverter.ToOpenApiAny(parameterDescription.DefaultValue);
            }

            parameter.Required |= parameterDescription.IsRequired;
        }
    }
}
