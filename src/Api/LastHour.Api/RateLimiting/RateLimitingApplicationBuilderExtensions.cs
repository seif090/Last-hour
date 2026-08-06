using Microsoft.Extensions.Options;

namespace LastHour.Api.RateLimiting;

/// <summary>
/// Adds the rate limiting middleware to the request pipeline when enabled by configuration.
/// </summary>
public static class RateLimitingApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the rate limiting middleware when the <see cref="RateLimitSettings.Enabled"/> setting
    /// is <see langword="true"/>. The middleware runs after routing so it can read endpoint
    /// metadata; the global limiter applies to every request and endpoints that opted into a
    /// named policy get their own limiter on top.
    /// </summary>
    /// <param name="app">The application builder to extend.</param>
    /// <returns>The same application builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
    public static IApplicationBuilder UseLastHourRateLimiting(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        RateLimitSettings settings =
            app.ApplicationServices.GetRequiredService<IOptions<RateLimitSettings>>().Value;

        if (settings.Enabled)
        {
            app.UseRateLimiter();
        }

        return app;
    }
}
