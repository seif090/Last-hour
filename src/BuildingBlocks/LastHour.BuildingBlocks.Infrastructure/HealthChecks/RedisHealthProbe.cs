using StackExchange.Redis;

namespace LastHour.BuildingBlocks.Infrastructure.HealthChecks;

/// <summary>
/// Pings the configured Redis instance to measure connectivity and round-trip latency.
/// </summary>
public sealed class RedisHealthProbe : IRedisHealthProbe
{
    private readonly IConnectionMultiplexer _multiplexer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisHealthProbe"/> class.
    /// </summary>
    /// <param name="multiplexer">The Redis connection multiplexer.</param>
    public RedisHealthProbe(IConnectionMultiplexer multiplexer)
    {
        _multiplexer = multiplexer;
    }

    /// <inheritdoc/>
    public Task<TimeSpan> PingAsync(CancellationToken cancellationToken = default)
        => _multiplexer.GetDatabase().PingAsync(CommandFlags.None).WaitAsync(cancellationToken);
}

/// <summary>
/// Abstraction over the Redis round-trip used by <see cref="RedisHealthCheck"/>, so the check
/// can be unit tested without a live Redis instance.
/// </summary>
public interface IRedisHealthProbe
{
    /// <summary>
    /// Pings the Redis instance and returns the round-trip latency.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The round-trip latency.</returns>
    Task<TimeSpan> PingAsync(CancellationToken cancellationToken = default);
}
