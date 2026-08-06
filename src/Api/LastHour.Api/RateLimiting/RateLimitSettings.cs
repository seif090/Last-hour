namespace LastHour.Api.RateLimiting;

/// <summary>
/// Settings that configure rate limiting. Bound from the
/// <see cref="SectionName"/> configuration section.
/// </summary>
public sealed class RateLimitSettings
{
    /// <summary>
    /// The configuration section the settings are bound from.
    /// </summary>
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Gets or sets a value indicating whether rate limiting is enabled.
    /// When disabled, no rate limiting services or middleware are registered.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the status code returned when a request is rejected. The RFC 6585
    /// <c>429 Too Many Requests</c> is the enterprise default, replacing the framework default
    /// of <c>503 Service Unavailable</c>.
    /// </summary>
    public int RejectionStatusCode { get; set; } = StatusCodes.Status429TooManyRequests;

    /// <summary>
    /// Gets or sets the name of the policy from <see cref="Policies"/> applied to every request
    /// through the global limiter.
    /// </summary>
    public string? GlobalPolicyName { get; set; }

    /// <summary>
    /// Gets the named policies registered with the rate limiter. Policies are opt-in per
    /// endpoint unless selected as the global limiter.
    /// </summary>
    public List<RateLimitPolicySettings> Policies { get; } = new List<RateLimitPolicySettings>();
}
