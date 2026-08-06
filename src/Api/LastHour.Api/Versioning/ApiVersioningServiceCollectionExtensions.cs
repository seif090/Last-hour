using Asp.Versioning;

namespace LastHour.Api.Versioning;

/// <summary>
/// Registers the LastHour API versioning surface: URL segment versioning, a default version
/// of 1.0, and the versioned API explorer that feeds the per-version Swagger documents.
/// </summary>
public static class ApiVersioningServiceCollectionExtensions
{
    /// <summary>
    /// Configures API versioning and the versioned API explorer used to build per-version
    /// Swagger documents.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddLastHourApiVersioning(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = ApiVersions.V1;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }
}
