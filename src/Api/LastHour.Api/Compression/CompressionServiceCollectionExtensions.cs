using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;

namespace LastHour.Api.Compression;

/// <summary>
/// Registers the response compression surface: Brotli and Gzip providers configured
/// from the <see cref="ResponseCompressionSettings.SectionName"/> configuration section.
/// Brotli is registered first so it is preferred whenever the client advertises support.
/// </summary>
public static class CompressionServiceCollectionExtensions
{
    /// <summary>
    /// Registers Brotli and Gzip response compression providers bound from configuration.
    /// No compression services are registered when compression is disabled by configuration.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The configuration to bind the settings from.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddLastHourResponseCompression(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        ResponseCompressionSettings settings =
            configuration.GetSection(ResponseCompressionSettings.SectionName).Get<ResponseCompressionSettings>() ?? new ResponseCompressionSettings();

        services.AddOptions<ResponseCompressionSettings>()
            .Bind(configuration.GetSection(ResponseCompressionSettings.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<ResponseCompressionSettings>, ResponseCompressionSettingsValidator>();

        if (settings.Enabled)
        {
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = settings.EnableForHttps;

                options.MimeTypes = ResponseCompressionDefaults.MimeTypes
                    .Concat(settings.MimeTypes)
                    .Distinct(StringComparer.OrdinalIgnoreCase);
            });

            services.Configure<BrotliCompressionProviderOptions>(provider => provider.Level = settings.CompressionLevel);
            services.Configure<GzipCompressionProviderOptions>(provider => provider.Level = settings.CompressionLevel);
        }

        return services;
    }
}
