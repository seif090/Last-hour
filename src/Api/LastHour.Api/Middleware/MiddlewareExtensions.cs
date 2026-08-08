using Microsoft.Extensions.Options;

namespace LastHour.Api.Middleware;

/// <summary>
/// Registers the request pipeline concerns that back the enterprise exception middleware:
/// the correlation id middleware and the exception handler segment.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Registers the middleware components used by the request pipeline.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The application configuration used to bind options.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/>
    /// is <see langword="null"/>.</exception>
    public static IServiceCollection AddLastHourMiddleware(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<CorrelationIdOptions>()
            .Bind(configuration.GetSection(CorrelationIdOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<CorrelationIdOptions>, CorrelationIdOptionsValidator>();

        services.AddScoped<CorrelationIdMiddleware>();

        return services;
    }

    /// <summary>
    /// Adds the enterprise exception middleware segment: the correlation id middleware runs first
    /// so every downstream component can resolve the request's correlation id, followed by the
    /// exception handler that converts unhandled exceptions into RFC 7807 problem details responses.
    /// </summary>
    /// <param name="app">The application builder to extend.</param>
    /// <returns>The same application builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
    public static IApplicationBuilder UseLastHourExceptionMiddleware(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseExceptionHandler();

        return app;
    }
}
