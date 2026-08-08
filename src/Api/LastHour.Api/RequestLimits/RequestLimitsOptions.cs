namespace LastHour.Api.RequestLimits;

/// <summary>
/// Configures the HTTP server request limits: the maximum request body size, header and keep-alive
/// timeouts, the multipart body length limit and the slow-client request data rate. Explicit limits
/// harden the surface against oversized payloads, slowloris-style connections and resource-exhaustion
/// attempts. <see langword="null"/> values leave the ASP.NET Core defaults untouched.
/// </summary>
public sealed class RequestLimitsOptions
{
    /// <summary>
    /// The configuration section the options are bound from.
    /// </summary>
    public const string SectionName = "RequestLimits";

    /// <summary>
    /// Gets or sets a value indicating whether the explicit limits are applied. When disabled the
    /// ASP.NET Core defaults apply unchanged.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum size of a request body, in bytes. <see langword="null"/> keeps the
    /// Kestrel default (30 MB).
    /// </summary>
    public long? MaxRequestBodySize { get; set; } = 10L * 1024 * 1024;

    /// <summary>
    /// Gets or sets the maximum time to receive the request headers. <see langword="null"/> keeps
    /// the Kestrel default (30 seconds).
    /// </summary>
    public TimeSpan? RequestHeadersTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the keep-alive timeout after which idle connections are closed. <see langword="null"/>
    /// keeps the Kestrel default (130 seconds).
    /// </summary>
    public TimeSpan? KeepAliveTimeout { get; set; } = TimeSpan.FromSeconds(130);

    /// <summary>
    /// Gets or sets the maximum size of a multipart request body field, in bytes. <see langword="null"/>
    /// keeps the framework default (128 MB).
    /// </summary>
    public long? MultipartBodyLengthLimit { get; set; } = 128L * 1024 * 1024;

    /// <summary>
    /// Gets or sets the minimum request body data rate, in bytes per second. <see langword="null"/>
    /// keeps the Kestrel default (240 bytes per second).
    /// </summary>
    public double? MinRequestBodyDataRateBytesPerSecond { get; set; } = 240;

    /// <summary>
    /// Gets or sets the grace period before the minimum request body data rate is enforced.
    /// <see langword="null"/> keeps the Kestrel default (5 seconds).
    /// </summary>
    public TimeSpan? MinRequestBodyDataRateGracePeriod { get; set; } = TimeSpan.FromSeconds(5);
}
