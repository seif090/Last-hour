namespace LastHour.Api.Caching.OutputCache;

/// <summary>
/// Settings that configure output caching. Bound from the
/// <see cref="SectionName"/> configuration section.
/// </summary>
public sealed class OutputCacheSettings
{
    /// <summary>
    /// The configuration section the settings are bound from.
    /// </summary>
    public const string SectionName = "OutputCache";

    /// <summary>
    /// Gets or sets a value indicating whether output caching is enabled.
    /// When disabled, no output cache services or middleware are registered.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether cache keys distinguish between different
    /// path casing, for example <c>/Widgets</c> and <c>/widgets</c>. The framework default
    /// treats request paths as case-insensitive.
    /// </summary>
    public bool UseCaseSensitivePaths { get; set; }

    /// <summary>
    /// Gets the named cache profiles registered as output cache policies. Nothing is cached
    /// until an endpoint opts into one of these profiles.
    /// </summary>
    public List<OutputCacheProfile> Profiles { get; } = new List<OutputCacheProfile>();
}
