using Microsoft.AspNetCore.OutputCaching;

namespace LastHour.Api.Caching.OutputCache;

/// <summary>
/// Registers the output cache surface: named cache profiles bound from the
/// <see cref="OutputCacheSettings.SectionName"/> configuration section.
/// </summary>
public static class OutputCacheServiceCollectionExtensions
{
    /// <summary>
    /// Registers output caching and binds the named cache profiles from configuration.
    /// Nothing is cached until an endpoint opts into a profile with <c>CacheOutput(profileName)</c>.
    /// The framework default policy never caches authenticated requests, requests carrying an
    /// Authorization header, non-GET/HEAD requests or responses other than 200 OK, which covers
    /// the requirement that authenticated endpoints are never cached.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The configuration to bind the settings from.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddLastHourOutputCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        OutputCacheSettings settings =
            configuration.GetSection(OutputCacheSettings.SectionName).Get<OutputCacheSettings>() ?? new OutputCacheSettings();

        services.Configure<OutputCacheSettings>(configuration.GetSection(OutputCacheSettings.SectionName));

        if (settings.Enabled)
        {
            services.AddOutputCache(options =>
            {
                options.UseCaseSensitivePaths = settings.UseCaseSensitivePaths;
                ConfigureProfiles(options, settings.Profiles);
            });
        }

        return services;
    }

    private static void ConfigureProfiles(OutputCacheOptions options, IEnumerable<OutputCacheProfile> profiles)
    {
        foreach (OutputCacheProfile profile in profiles)
        {
            if (string.IsNullOrWhiteSpace(profile.Name))
            {
                continue;
            }

            options.AddPolicy(profile.Name, builder => ApplyProfile(builder, profile));
        }
    }

    private static void ApplyProfile(OutputCachePolicyBuilder builder, OutputCacheProfile profile)
    {
        if (profile.DurationSeconds is int durationSeconds)
        {
            builder.Expire(TimeSpan.FromSeconds(durationSeconds));
        }

        if (profile.VaryByQueryKeys is { Length: > 0 } queryKeys)
        {
            builder.SetVaryByQuery(queryKeys);
        }

        if (profile.VaryByHeaderNames is { Length: > 0 } headerNames)
        {
            builder.SetVaryByHeader(headerNames);
        }

        if (profile.VaryByRouteValueNames is { Length: > 0 } routeValueNames)
        {
            builder.SetVaryByRouteValue(routeValueNames);
        }

        if (profile.Tags is { Length: > 0 } tags)
        {
            builder.Tag(tags);
        }

        if (profile.VaryByHost)
        {
            builder.SetVaryByHost(true);
        }
    }
}
