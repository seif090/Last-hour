using Microsoft.Extensions.Options;

namespace LastHour.Api.Security.SecurityHeaders;

/// <summary>
/// Registers the security headers middleware and its configuration.
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// Registers the security headers options and middleware.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The application configuration used to bind options.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/>
    /// is <see langword="null"/>.</exception>
    public static IServiceCollection AddLastHourSecurityHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<SecurityHeadersOptions>()
            .Bind(configuration.GetSection(SecurityHeadersOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<SecurityHeadersOptions>, SecurityHeadersOptionsValidator>();

        services.AddScoped<SecurityHeadersMiddleware>();

        return services;
    }

    /// <summary>
    /// Adds the security headers middleware to the pipeline, after the exception handler and
    /// HTTPS redirection so error responses are also protected and HSTS runs over HTTPS.
    /// </summary>
    /// <param name="app">The application builder to extend.</param>
    /// <returns>The same application builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
    public static IApplicationBuilder UseLastHourSecurityHeaders(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<SecurityHeadersMiddleware>();

        return app;
    }
}
