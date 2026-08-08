using Microsoft.Extensions.Options;

namespace LastHour.Api.Secrets;

/// <summary>
/// Registers the secrets surface: the <see cref="SecretsOptions"/> are bound and validated, the
/// <see cref="ISecretProvider"/> is registered for application code, and a configuration source
/// overlays declared secret values onto the configuration root.
/// </summary>
public static class SecretsServiceCollectionExtensions
{
    /// <summary>
    /// Binds and validates the secrets options and registers the <see cref="ISecretProvider"/>.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The application configuration used to bind settings.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/>
    /// is <see langword="null"/>.</exception>
    public static IServiceCollection AddLastHourSecrets(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<SecretsOptions>()
            .Bind(configuration.GetSection(SecretsOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<SecretsOptions>, SecretsOptionsValidator>();
        services.AddSingleton<ISecretProvider>(provider =>
        {
            SecretsOptions options = provider.GetRequiredService<IOptions<SecretsOptions>>().Value;
            return new EnvironmentSecretProvider(options);
        });

        return services;
    }

    /// <summary>
    /// Adds the secret values declared in the <see cref="SecretsOptions.SectionName"/> section to
    /// the configuration. The source is appended last so secret values take precedence over the
    /// application settings files.
    /// </summary>
    /// <param name="configuration">The configuration manager to extend.</param>
    /// <returns>The same configuration manager, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static ConfigurationManager AddLastHourSecrets(this ConfigurationManager configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        SecretsOptions options =
            configuration.GetSection(SecretsOptions.SectionName).Get<SecretsOptions>() ?? new SecretsOptions();
        ((IConfigurationBuilder)configuration).Add(new SecretsConfigurationSource(options));

        return configuration;
    }
}
