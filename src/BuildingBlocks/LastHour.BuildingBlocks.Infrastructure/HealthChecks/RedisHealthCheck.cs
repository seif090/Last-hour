using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.HealthChecks;

/// <summary>
/// Reports the health of the Redis cache by measuring a round-trip ping, subject to the
/// configured per-check timeout.
/// </summary>
public sealed class RedisHealthCheck : IHealthCheck
{
    private readonly IRedisHealthProbe _probe;
    private readonly IOptions<HealthChecksOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisHealthCheck"/> class.
    /// </summary>
    /// <param name="probe">The Redis round-trip probe.</param>
    /// <param name="options">The health check options.</param>
    public RedisHealthCheck(IRedisHealthProbe probe, IOptions<HealthChecksOptions> options)
    {
        _probe = probe;
        _options = options;
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_options.Value.TimeoutSeconds));

            TimeSpan latency = await _probe.PingAsync(timeout.Token).ConfigureAwait(false);
            return HealthCheckResult.Healthy($"Redis ping round-trip: {latency.TotalMilliseconds:F0} ms.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis ping failed.", ex);
        }
    }
}
