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
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddLastHourMiddleware(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

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
