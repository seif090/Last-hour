namespace LastHour.Api.Security.ForwardedHeaders;

/// <summary>
/// Configures how the API trusts forwarded headers from a reverse proxy or load balancer.
/// The secure default forwards all standard headers but trusts only the immediate hop
/// (<see cref="ForwardLimit"/> of one with no known proxies). Production deployments behind a
/// proxy should pin <see cref="KnownProxies"/> or <see cref="KnownNetworks"/> and raise the
/// limit accordingly so spoofed client addresses cannot be injected.
/// </summary>
public sealed class ForwardedHeadersSettings
{
    /// <summary>
    /// Gets the name of the configuration section the settings bind from.
    /// </summary>
    public const string SectionName = "ForwardedHeaders";

    /// <summary>
    /// Gets or sets a value indicating whether forwarded headers are processed.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the forwarded header fields that are processed.
    /// </summary>
    public Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders ForwardedHeaders { get; set; } =
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All;

    /// <summary>
    /// Gets or sets the maximum number of forwarding hops to process.
    /// </summary>
    public int ForwardLimit { get; set; } = 1;

    /// <summary>
    /// Gets or sets the IP addresses of the proxies that are allowed to send forwarded headers.
    /// </summary>
    public string[] KnownProxies { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the networks (in CIDR notation, for example <c>10.0.0.0/8</c>) that are
    /// allowed to send forwarded headers.
    /// </summary>
    public string[] KnownNetworks { get; set; } = Array.Empty<string>();
}
