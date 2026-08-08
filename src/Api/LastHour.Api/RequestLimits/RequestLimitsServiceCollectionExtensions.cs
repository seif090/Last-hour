using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

namespace LastHour.Api.RequestLimits;

/// <summary>
/// Registers the HTTP server request limits: the <see cref="RequestLimitsOptions"/> are bound and
/// validated, then applied to the Kestrel server limits and the multipart form options.
/// </summary>
public static class RequestLimitsServiceCollectionExtensions
{
    /// <summary>
    /// Binds and validates the request limits options and applies them to Kestrel and the form
    /// parsing options. Nothing is applied when limits are disabled.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The application configuration used to bind settings.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/>
    /// is <see langword="null"/>.</exception>
    public static IServiceCollection AddLastHourRequestLimits(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<RequestLimitsOptions>()
            .Bind(configuration.GetSection(RequestLimitsOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<RequestLimitsOptions>, RequestLimitsOptionsValidator>();

        services.Configure<KestrelServerOptions>(kestrel =>
        {
            RequestLimitsOptions settings = configuration.GetSection(RequestLimitsOptions.SectionName)
                .Get<RequestLimitsOptions>() ?? new RequestLimitsOptions();

            if (!settings.Enabled)
            {
                return;
            }

            if (settings.MaxRequestBodySize is long maxBody)
            {
                kestrel.Limits.MaxRequestBodySize = maxBody;
            }

            if (settings.RequestHeadersTimeout is TimeSpan headers)
            {
                kestrel.Limits.RequestHeadersTimeout = headers;
            }

            if (settings.KeepAliveTimeout is TimeSpan keepAlive)
            {
                kestrel.Limits.KeepAliveTimeout = keepAlive;
            }

            if (settings.MinRequestBodyDataRateBytesPerSecond is double rate
                && settings.MinRequestBodyDataRateGracePeriod is TimeSpan grace)
            {
                kestrel.Limits.MinRequestBodyDataRate = new MinDataRate(rate, grace);
            }
        });

        services.Configure<FormOptions>(form =>
        {
            RequestLimitsOptions settings = configuration.GetSection(RequestLimitsOptions.SectionName)
                .Get<RequestLimitsOptions>() ?? new RequestLimitsOptions();

            if (!settings.Enabled)
            {
                return;
            }

            if (settings.MultipartBodyLengthLimit is long multipart)
            {
                form.MultipartBodyLengthLimit = multipart;
            }
        });

        return services;
    }
}
