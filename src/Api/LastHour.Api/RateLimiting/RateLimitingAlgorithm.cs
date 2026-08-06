namespace LastHour.Api.RateLimiting;

/// <summary>
/// The rate limiting algorithms a policy can use. Bound from the <c>Algorithm</c> property of
/// each <see cref="RateLimitPolicySettings"/> entry in configuration.
/// </summary>
public enum RateLimitingAlgorithm
{
    /// <summary>
    /// A fixed window limiter that permits a fixed number of requests per window, resetting
    /// the count when the window elapses. Subject to burst patterns at window boundaries.
    /// </summary>
    FixedWindow,

    /// <summary>
    /// A sliding window limiter that smooths the budget over sub-segments of the window,
    /// avoiding the burst-at-boundary behavior of the fixed window.
    /// </summary>
    SlidingWindow,

    /// <summary>
    /// A token bucket limiter that refills tokens at a steady rate.
    /// </summary>
    TokenBucket,

    /// <summary>
    /// A concurrency limiter that caps the number of requests in flight rather than the
    /// request rate.
    /// </summary>
    Concurrency,

    /// <summary>
    /// Disables limiting for the partition; requests pass through unrestricted.
    /// </summary>
    NoLimit,
}
