namespace LastHour.Api.Secrets;

/// <summary>
/// A configuration source that overlays secret values onto the configuration root. Each name
/// declared in <see cref="SecretsOptions.Names"/> is resolved through <see cref="ISecretProvider"/>
/// and, when a value exists, overrides the application settings file value. Because it is added
/// last, secret values always win over appsettings.
/// </summary>
public sealed class SecretsConfigurationSource : IConfigurationSource
{
    private readonly SecretsOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecretsConfigurationSource"/> class.
    /// </summary>
    /// <param name="options">The secrets configuration describing which keys are secrets.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public SecretsConfigurationSource(SecretsOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new SecretsConfigurationProvider(_options, new EnvironmentSecretProvider(_options));
    }

    private sealed class SecretsConfigurationProvider : ConfigurationProvider
    {
        private readonly SecretsOptions _options;
        private readonly ISecretProvider _secrets;

        public SecretsConfigurationProvider(SecretsOptions options, ISecretProvider secrets)
        {
            _options = options;
            _secrets = secrets;
        }

        public override void Load()
        {
            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            if (_options.Enabled)
            {
                foreach (string name in _options.Names)
                {
                    string? value = _secrets.GetSecret(name);
                    if (value is not null)
                    {
                        values[name] = value;
                    }
                }
            }

            Data = values;
        }
    }
}
