namespace LastHour.Api.RateLimiting;

/// <summary>
/// The strategy used to derive the partition key of a rate limiting policy. Each distinct key
/// receives its own independent limiter budget. Bound from the <c>PartitionBy</c> property of
/// each <see cref="RateLimitPolicySettings"/> entry in configuration.
/// </summary>
public enum RateLimitingPartitioning
{
    /// <summary>
    /// A single partition shared by every request. Used for global limiters so all traffic
    /// draws from one budget.
    /// </summary>
    Global,

    /// <summary>
    /// One partition per client IP address, identified by
    /// <c>HttpContext.Connection.RemoteIpAddress</c>.
    /// </summary>
    IpAddress,

    /// <summary>
    /// One partition per request header value, identified by the configured
    /// <see cref="RateLimitPolicySettings.PartitionHeaderName"/>.
    /// </summary>
    Header,

    /// <summary>
    /// One partition per request path.
    /// </summary>
    Path,

    /// <summary>
    /// One partition per request host.
    /// </summary>
    Host,
}
