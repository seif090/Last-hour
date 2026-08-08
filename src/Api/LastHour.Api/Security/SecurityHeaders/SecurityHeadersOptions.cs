namespace LastHour.Api.Security.SecurityHeaders;

/// <summary>
/// Configures the security headers applied to every response. The defaults follow OWASP
/// guidance: <c>X-Content-Type-Options: nosniff</c>, a strict referrer policy, frame denial,
/// a restrictive permissions policy and a <c>default-src 'none'</c> content security policy.
/// HSTS is only sent over HTTPS. <c>X-XSS-Protection</c> is deliberately not sent because it is
/// deprecated and can introduce client-side vulnerabilities; the CSP is the defense in depth.
/// </summary>
public sealed class SecurityHeadersOptions
{
    /// <summary>
    /// Gets the name of the configuration section the options bind from.
    /// </summary>
    public const string SectionName = "SecurityHeaders";

    /// <summary>
    /// Gets or sets a value indicating whether security headers are applied at all.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether <c>X-Content-Type-Options: nosniff</c> is sent.
    /// </summary>
    public bool XContentTypeOptionsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the value of the <c>Referrer-Policy</c> header.
    /// </summary>
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

    /// <summary>
    /// Gets or sets the value of the <c>X-Frame-Options</c> header.
    /// </summary>
    public string FrameOptions { get; set; } = "DENY";

    /// <summary>
    /// Gets or sets the value of the <c>Permissions-Policy</c> header.
    /// </summary>
    public string PermissionsPolicy { get; set; } = "camera=(), microphone=(), geolocation=()";

    /// <summary>
    /// Gets or sets the value of the <c>Content-Security-Policy</c> header. The production default
    /// (<c>default-src 'none'</c>) blocks all browser resources; development overrides it to allow
    /// the Swagger UI assets.
    /// </summary>
    public string ContentSecurityPolicy { get; set; } = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'";

    /// <summary>
    /// Gets or sets the HTTP Strict Transport Security configuration.
    /// </summary>
    public HstsOptions Hsts { get; set; } = new HstsOptions();
}

/// <summary>
/// Configures the HTTP Strict Transport Security header.
/// </summary>
public sealed class HstsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the HSTS header is sent (over HTTPS only).
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the HSTS <c>max-age</c> in days.
    /// </summary>
    public int MaxAgeDays { get; set; } = 365;

    /// <summary>
    /// Gets or sets a value indicating whether <c>includeSubDomains</c> is added.
    /// </summary>
    public bool IncludeSubDomains { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the <c>preload</c> directive is added. Only enable
    /// after the domain is submitted to the HSTS preload list.
    /// </summary>
    public bool Preload { get; set; }
}
