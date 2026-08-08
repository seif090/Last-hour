using LastHour.BuildingBlocks.Infrastructure.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.HealthChecks;

public class RedisHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_SuccessfulPing_ReturnsHealthy()
    {
        var check = new RedisHealthCheck(
            new FakeRedisHealthProbe(() => Task.FromResult(TimeSpan.FromMilliseconds(1))),
            Options.Create(new HealthChecksOptions()));

        HealthCheckResult result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_FailingPing_ReturnsUnhealthy()
    {
        var check = new RedisHealthCheck(
            new FakeRedisHealthProbe(() => Task.FromException<TimeSpan>(new TimeoutException("connection timed out"))),
            Options.Create(new HealthChecksOptions()));

        HealthCheckResult result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    private sealed class FakeRedisHealthProbe : IRedisHealthProbe
    {
        private readonly Func<Task<TimeSpan>> _ping;

        public FakeRedisHealthProbe(Func<Task<TimeSpan>> ping)
        {
            _ping = ping;
        }

        public Task<TimeSpan> PingAsync(CancellationToken cancellationToken = default)
            => _ping();
    }
}
