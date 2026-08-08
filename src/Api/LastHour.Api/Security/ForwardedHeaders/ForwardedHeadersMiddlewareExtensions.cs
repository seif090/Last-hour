using System.Globalization;
using System.Net;
using Microsoft.Extensions.Options;

namespace LastHour.Api.Security.ForwardedHeaders;

/// <summary>
/// Registers the forwarded headers handling: the settings are bound and validated, then mapped
/// onto the ASP.NET Core forwarded headers options. HTTPS redirection and the remote IP address
/// reported in request logs then see the scheme, host and client address the proxy forwarded.
/// </summary>
public static class ForwardedHeadersMiddlewareExtensions
{
    /// <summary>
    /// Registers the forwarded headers options and maps them onto the ASP.NET Core configuration.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The application configuration used to bind settings.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/>
    /// is <see langword="null"/>.</exception>
    public static IServiceCollection AddLastHourForwardedHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<ForwardedHeadersSettings>()
            .Bind(configuration.GetSection(ForwardedHeadersSettings.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<ForwardedHeadersSettings>, ForwardedHeadersSettingsValidator>();

        services.Configure<ForwardedHeadersOptions>(forwarded =>
        {
            forwarded.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.None;
            forwarded.KnownProxies.Clear();
            forwarded.KnownNetworks.Clear();

            ForwardedHeadersSettings settings = configuration.GetSection(ForwardedHeadersSettings.SectionName)
                .Get<ForwardedHeadersSettings>() ?? new ForwardedHeadersSettings();

            if (!settings.Enabled)
            {
                return;
            }

            forwarded.ForwardedHeaders = settings.ForwardedHeaders;
            forwarded.ForwardLimit = settings.ForwardLimit;

            foreach (string proxy in settings.KnownProxies)
            {
                forwarded.KnownProxies.Add(IPAddress.Parse(proxy));
            }

            foreach (string network in settings.KnownNetworks)
            {
                string[] parts = network.Split('/');
                var address = IPAddress.Parse(parts[0]);
                int prefixLength = parts.Length > 1
                    ? int.Parse(parts[1], CultureInfo.InvariantCulture)
                    : 32;
                forwarded.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(address, prefixLength));
            }
        });

        return services;
    }

    /// <summary>
    /// Adds the forwarded headers middleware to the pipeline. It must run before HTTPS redirection
    /// and the request logger so the forwarded scheme, host and client address are honored.
    /// </summary>
    /// <param name="app">The application builder to extend.</param>
    /// <returns>The same application builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
    public static IApplicationBuilder UseLastHourForwardedHeaders(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseForwardedHeaders();

        return app;
    }
}
