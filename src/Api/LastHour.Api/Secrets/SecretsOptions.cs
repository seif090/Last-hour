namespace LastHour.Api.Secrets;

/// <summary>
/// Configures the secrets surface. The <c>Secrets</c> section declares which configuration keys
/// are considered secrets; their values are sourced from the configured provider instead of the
/// application settings files, so they never reach source control. Binding is case-insensitive,
/// matching the configuration system.
/// </summary>
public sealed class SecretsOptions
{
    /// <summary>
    /// The configuration section the options are bound from.
    /// </summary>
    public const string SectionName = "Secrets";

    /// <summary>
    /// Gets or sets a value indicating whether secret resolution is enabled. When disabled the
    /// secret configuration source adds nothing and the registered provider returns nothing.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the environment variable prefix used by the environment provider. The default
    /// keeps the variables grouped and unlikely to collide with other software.
    /// </summary>
    public string EnvironmentVariablePrefix { get; set; } = "LASTHOUR_SECRET_";

    /// <summary>
    /// Gets or sets the configuration keys treated as secrets, for example
    /// <c>ConnectionStrings:Postgres</c>. Each key present in the secret source overrides any
    /// value from the application settings files.
    /// </summary>
    public string[] Names { get; set; } = Array.Empty<string>();
}
