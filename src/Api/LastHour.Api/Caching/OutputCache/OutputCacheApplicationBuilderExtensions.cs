using Microsoft.Extensions.Options;

namespace LastHour.Api.Caching.OutputCache;

/// <summary>
/// Adds the output cache middleware to the request pipeline when enabled by configuration.
/// </summary>
public static class OutputCacheApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the output cache middleware when the <see cref="OutputCacheSettings.Enabled"/>
    /// setting is <see langword="true"/>. The middleware runs after routing so it can read the
    /// endpoint metadata; only endpoints that opted into a cache profile are considered for caching.
    /// </summary>
    /// <param name="app">The application builder to extend.</param>
    /// <returns>The same application builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
    public static IApplicationBuilder UseLastHourOutputCache(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        OutputCacheSettings settings =
            app.ApplicationServices.GetRequiredService<IOptions<OutputCacheSettings>>().Value;

        if (settings.Enabled)
        {
            app.UseOutputCache();
        }

        return app;
    }
}
