namespace LastHour.Api.Security.Cors;

/// <summary>
/// Configures the CORS policy. The policy is configuration driven per environment: development
/// sets <see cref="AllowAnyOrigin"/> (wide open, no credentials) while production lists explicit
/// <see cref="AllowedOrigins"/> and disallows <c>AllowAnyOrigin</c>. The validator rejects a
/// production configuration that would allow any origin, so the guarantee cannot be weakened by
/// a bad setting.
/// </summary>
public sealed class CorsOptions
{
    /// <summary>
    /// Gets the name of the configuration section the options bind from.
    /// </summary>
    public const string SectionName = "Cors";

    /// <summary>
    /// Gets or sets a value indicating whether CORS is enabled at all.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether any origin is allowed. Development only; the
    /// production environment forbids this.
    /// </summary>
    public bool AllowAnyOrigin { get; set; }

    /// <summary>
    /// Gets or sets the origins allowed to call the API. Wildcard subdomains are supported with
    /// the <c>https://*.example.com</c> syntax.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the HTTP methods allowed on cross-origin requests.
    /// </summary>
    public string[] AllowedMethods { get; set; } = { "GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS" };

    /// <summary>
    /// Gets or sets the request headers allowed on cross-origin requests.
    /// </summary>
    public string[] AllowedHeaders { get; set; } = { "Content-Type", "Accept", "Authorization", "X-Correlation-ID" };

    /// <summary>
    /// Gets or sets a value indicating whether credentials (cookies and authorization headers)
    /// are allowed on cross-origin requests. Must remain <see langword="false"/> when
    /// <see cref="AllowAnyOrigin"/> is used.
    /// </summary>
    public bool AllowCredentials { get; set; }
}
