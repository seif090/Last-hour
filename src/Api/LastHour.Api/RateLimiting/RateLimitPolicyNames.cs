namespace LastHour.Api.RateLimiting;

/// <summary>
/// The canonical names of the rate limiting policies shipped with the API. Configuration in the
/// <see cref="RateLimitSettings.SectionName"/> section must use these names so endpoints and the
/// global limiter resolve the same policies.
/// </summary>
public static class RateLimitPolicyNames
{
    /// <summary>
    /// The policy applied to every request through the global limiter.
    /// </summary>
    public const string Global = "Global";

    /// <summary>
    /// The policy applied to the system status endpoint.
    /// </summary>
    public const string SystemStatus = "SystemStatus";
}
