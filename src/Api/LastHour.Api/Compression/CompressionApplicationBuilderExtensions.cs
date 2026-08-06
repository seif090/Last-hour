using Microsoft.Extensions.Options;

namespace LastHour.Api.Compression;

/// <summary>
/// Adds the response compression middleware to the request pipeline when enabled by configuration.
/// </summary>
public static class CompressionApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the response compression middleware when the <see cref="ResponseCompressionSettings.Enabled"/>
    /// setting is <see langword="true"/>. The middleware negotiates the best provider (Brotli preferred)
    /// for each request and compresses eligible responses before the endpoint layer writes them.
    /// </summary>
    /// <param name="app">The application builder to extend.</param>
    /// <returns>The same application builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
    public static IApplicationBuilder UseLastHourResponseCompression(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        ResponseCompressionSettings settings =
            app.ApplicationServices.GetRequiredService<IOptions<ResponseCompressionSettings>>().Value;

        if (settings.Enabled)
        {
            app.UseResponseCompression();
        }

        return app;
    }
}
