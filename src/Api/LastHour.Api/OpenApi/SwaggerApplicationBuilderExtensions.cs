using Asp.Versioning;
using LastHour.Api.Versioning;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace LastHour.Api.OpenApi;

/// <summary>
/// Adds the Swagger middleware: the raw Swagger JSON endpoints and the Swagger UI that lists one
/// document per API version with a production-grade layout (collapsed operations, deep linking,
/// request duration, filtering and model examples). Intended for development environments only.
/// </summary>
public static class SwaggerApplicationBuilderExtensions
{
    /// <summary>
    /// Enables the Swagger JSON endpoints and Swagger UI.
    /// </summary>
    /// <param name="app">The application builder to extend.</param>
    /// <returns>The same application builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
    public static IApplicationBuilder UseLastHourSwagger(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            foreach (ApiVersion version in ApiVersions.Supported)
            {
                options.SwaggerEndpoint(
                    $"/swagger/v{version.MajorVersion}/swagger.json",
                    $"LastHour API v{version.MajorVersion}");
            }

            options.DocExpansion(DocExpansion.None);
            options.EnableDeepLinking();
            options.DisplayRequestDuration();
            options.EnableFilter();
            options.ShowCommonExtensions();
            options.DefaultModelRendering(ModelRendering.Example);
            options.DefaultModelsExpandDepth(1);
        });

        return app;
    }
}
