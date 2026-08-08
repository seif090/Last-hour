namespace LastHour.Api.Secrets;

/// <summary>
/// Resolves secrets from environment variables. A secret named <c>ConnectionStrings:Postgres</c>
/// maps to the environment variable <c>LASTHOUR_SECRET_CONNECTIONSTRINGS_POSTGRES</c>; the prefix
/// is configurable through <see cref="SecretsOptions.EnvironmentVariablePrefix"/>. This is the
/// deployment-friendly default: secrets are injected by the platform, never stored in files.
/// </summary>
public sealed class EnvironmentSecretProvider : ISecretProvider
{
    private readonly SecretsOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentSecretProvider"/> class.
    /// </summary>
    /// <param name="options">The secrets configuration.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public EnvironmentSecretProvider(SecretsOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public string? GetSecret(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        string variable = EnvironmentVariableName(name);
        return Environment.GetEnvironmentVariable(variable);
    }

    private string EnvironmentVariableName(string name)
    {
        string normalized = name.Replace(':', '_').Replace('.', '_').ToUpperInvariant();
        return _options.EnvironmentVariablePrefix + normalized;
    }
}
