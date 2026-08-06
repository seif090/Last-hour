using System.IO.Compression;

namespace LastHour.Api.Compression;

/// <summary>
/// Settings that configure response compression. Bound from the
/// <see cref="SectionName"/> configuration section.
/// </summary>
public sealed class ResponseCompressionSettings
{
    /// <summary>
    /// The configuration section the settings are bound from.
    /// </summary>
    public const string SectionName = "ResponseCompression";

    /// <summary>
    /// Gets or sets a value indicating whether response compression is enabled.
    /// When disabled, no compression services or middleware are registered.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether responses are compressed over HTTPS.
    /// Enabled by default; the BREACH attack trade-off is accepted because the API
    /// does not embed secrets in responses.
    /// </summary>
    public bool EnableForHttps { get; set; } = true;

    /// <summary>
    /// Gets or sets the compression level applied by both the Brotli and Gzip providers.
    /// </summary>
    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.SmallestSize;

    /// <summary>
    /// Gets additional MIME types to compress beyond the framework defaults. The framework already
    /// covers <c>application/json</c>, <c>text/plain</c> and the other common text types; the API
    /// adds <c>application/problem+json</c> for RFC 7807 problem details responses.
    /// </summary>
    public List<string> MimeTypes { get; } = new List<string>();
}
