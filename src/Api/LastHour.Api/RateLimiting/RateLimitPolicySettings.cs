namespace LastHour.Api.RateLimiting;

/// <summary>
/// Describes a named rate limiting policy bound from the
/// <see cref="RateLimitSettings.SectionName"/> configuration section. The same policy shape
/// covers all <see cref="RateLimitingAlgorithm"/> values; unused members are ignored by the
/// selected algorithm.
/// </summary>
public sealed class RateLimitPolicySettings
{
    /// <summary>
    /// Gets or sets the name used to reference the policy, for example through the global limiter
    /// setting or <c>RequireRateLimiting(name)</c> on an endpoint.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the algorithm the policy uses to limit requests.
    /// </summary>
    public RateLimitingAlgorithm Algorithm { get; set; } = RateLimitingAlgorithm.FixedWindow;

    /// <summary>
    /// Gets or sets the strategy used to derive the partition key, giving every distinct key its
    /// own independent budget.
    /// </summary>
    public RateLimitingPartitioning PartitionBy { get; set; } = RateLimitingPartitioning.Global;

    /// <summary>
    /// Gets or sets the request header name used as the partition key when
    /// <see cref="PartitionBy"/> is <see cref="RateLimitingPartitioning.Header"/>.
    /// </summary>
    public string? PartitionHeaderName { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of requests allowed per window. Used by the fixed window,
    /// sliding window and concurrency algorithms.
    /// </summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>
    /// Gets or sets the window length in seconds. Used by the fixed window, sliding window and
    /// token bucket algorithms.
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the number of sub-segments the sliding window is divided into. Higher values
    /// smooth the budget more aggressively across the window.
    /// </summary>
    public int SegmentsPerWindow { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum number of requests queued when the budget is exhausted. Zero
    /// disables queueing so excess requests are rejected immediately.
    /// </summary>
    public int QueueLimit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the limiter replenishes its budget on a timer
    /// rather than on access.
    /// </summary>
    public bool AutoReplenishment { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum capacity of the token bucket.
    /// </summary>
    public int TokenLimit { get; set; }

    /// <summary>
    /// Gets or sets the number of tokens restored every <see cref="ReplenishmentPeriodSeconds"/>.
    /// </summary>
    public int TokensPerPeriod { get; set; }

    /// <summary>
    /// Gets or sets the token bucket replenishment period in seconds.
    /// </summary>
    public int ReplenishmentPeriodSeconds { get; set; } = 60;
}
