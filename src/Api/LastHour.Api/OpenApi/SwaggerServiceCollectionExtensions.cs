using Asp.Versioning;
using LastHour.Api.OpenApi.Filters;
using LastHour.Api.Versioning;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace LastHour.Api.OpenApi;

/// <summary>
/// Registers the Swagger generation surface: one Swagger document per supported API version, XML
/// documentation from every LastHour assembly, a JWT bearer security scheme applied to all
/// operations, operation filters that enrich parameters and error responses, schema-level
/// examples and feature-based tag grouping.
/// </summary>
public static class SwaggerServiceCollectionExtensions
{
    /// <summary>
    /// Configures Swagger generation for the versioned LastHour API.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddLastHourSwagger(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSwaggerGen(options =>
        {
            foreach (ApiVersion version in ApiVersions.Supported)
            {
                options.SwaggerDoc($"v{version.MajorVersion}", SwaggerInfoFactory.Create(version));
            }

            foreach (string xmlPath in Directory.GetFiles(AppContext.BaseDirectory, "LastHour.*.xml"))
            {
                options.IncludeXmlComments(xmlPath);
            }

            var bearerScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "A JWT issued by the identity module. " +
                              "Paste the token as `Bearer <token>` or click Authorize and enter the raw token.",
            };

            var bearerRequirement = new OpenApiSecurityRequirement();
            bearerRequirement.Add(
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                },
                Array.Empty<string>());

            options.AddSecurityDefinition("Bearer", bearerScheme);
            options.AddSecurityRequirement(bearerRequirement);

            options.OperationFilter<SwaggerDefaultValuesOperationFilter>();
            options.OperationFilter<CorrelationIdOperationFilter>();
            options.OperationFilter<ProblemDetailsDemoOperationFilter>();
            options.SchemaFilter<EndpointExampleSchemaFilter>();

            options.OrderActionsBy(apiDescription => apiDescription.RelativePath);
            options.TagActionsBy(apiDescription => ResolveTags(apiDescription));
        });

        return services;
    }

    private static string[] ResolveTags(ApiDescription apiDescription)
    {
        string[] tags = apiDescription.ActionDescriptor.EndpointMetadata
            .OfType<ITagsMetadata>()
            .SelectMany(metadata => metadata.Tags)
            .ToArray();

        return tags.Length > 0 ? tags : new[] { apiDescription.GroupName ?? "General" };
    }
}
