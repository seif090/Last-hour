namespace LastHour.Api.Caching.OutputCache;

/// <summary>
/// Describes a named output cache profile bound from the
/// <see cref="OutputCacheSettings.SectionName"/> configuration section.
/// Each profile becomes a named output cache policy that endpoints opt into
/// through <c>CacheOutput(profileName)</c>.
/// </summary>
public sealed class OutputCacheProfile
{
    /// <summary>
    /// The name of the default profile used by the service root endpoint.
    /// </summary>
    public const string DefaultName = "Default";

    /// <summary>
    /// Gets or sets the name used to reference the profile, for example through
    /// <c>CacheOutput(name)</c> on an endpoint.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of seconds a cached response remains valid before it expires.
    /// When not set, the framework default expiration time span applies.
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Gets or sets the query string keys to vary cached responses by. An entry of <c>*</c>
    /// varies by every query key, which is the framework default. Empty when not configured.
    /// </summary>
    public string[] VaryByQueryKeys { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the request header names to vary cached responses by. Empty when not configured.
    /// </summary>
    public string[] VaryByHeaderNames { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the route value names to vary cached responses by. Empty when not configured.
    /// </summary>
    public string[] VaryByRouteValueNames { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets a value indicating whether cached responses vary by the Host header.
    /// </summary>
    public bool VaryByHost { get; set; }

    /// <summary>
    /// Gets or sets the tags attached to cached responses so entries can be invalidated
    /// as a group through <see cref="Microsoft.AspNetCore.OutputCaching.IOutputCacheStore"/>.
    /// Empty when not configured.
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();
}
