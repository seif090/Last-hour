namespace LastHour.Api.Observability.RequestLogging;

/// <summary>
/// Registers the request logging middleware that records a single structured event per HTTP
/// request with its method, path, status code, execution time, remote address, user agent,
/// correlation id, request id and authenticated user.
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    /// <summary>
    /// Registers the request logging middleware.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddLastHourRequestLogging(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<RequestLoggingMiddleware>();

        return services;
    }

    /// <summary>
    /// Adds the request logging middleware to the pipeline. It must run inside the exception
    /// handler so error responses are logged too, and after forwarded headers and compression
    /// so the reported address and status code are accurate.
    /// </summary>
    /// <param name="app">The application builder to extend.</param>
    /// <returns>The same application builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
    public static IApplicationBuilder UseLastHourRequestLogging(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<RequestLoggingMiddleware>();

        return app;
    }
}
