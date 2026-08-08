using Microsoft.Extensions.Options;

namespace LastHour.Api.Security.Cors;

/// <summary>
/// Registers the CORS policy and middleware. The policy is built from the <see cref="CorsOptions"/>
/// settings: development may allow any origin, production only the explicitly configured origins.
/// The middleware runs after the security headers so preflight responses are also protected.
/// </summary>
public static class CorsServiceCollectionExtensions
{
    private const string PolicyName = "LastHourCors";

    /// <summary>
    /// Registers the CORS options, their validation and the CORS policy.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The application configuration used to bind settings.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/>
    /// is <see langword="null"/>.</exception>
    public static IServiceCollection AddLastHourCors(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(CorsOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<CorsOptions>, CorsOptionsValidator>();

        services.AddCors(cors =>
        {
            cors.AddPolicy(PolicyName, builder =>
            {
                CorsOptions settings = configuration.GetSection(CorsOptions.SectionName)
                    .Get<CorsOptions>() ?? new CorsOptions();

                if (!settings.Enabled)
                {
                    return;
                }

                if (settings.AllowAnyOrigin)
                {
                    builder.AllowAnyOrigin();
                }
                else if (settings.AllowedOrigins.Length > 0)
                {
                    builder.WithOrigins(settings.AllowedOrigins);
                }

                if (settings.AllowedMethods.Length > 0)
                {
                    builder.WithMethods(settings.AllowedMethods);
                }

                if (settings.AllowedHeaders.Length > 0)
                {
                    builder.WithHeaders(settings.AllowedHeaders);
                }

                if (settings.AllowCredentials)
                {
                    builder.AllowCredentials();
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Adds the CORS middleware to the pipeline, after the security headers so preflight and
    /// cross-origin responses are protected, and before the request logger.
    /// </summary>
    /// <param name="app">The application builder to extend.</param>
    /// <returns>The same application builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
    public static IApplicationBuilder UseLastHourCors(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseCors(PolicyName);

        return app;
    }
}
