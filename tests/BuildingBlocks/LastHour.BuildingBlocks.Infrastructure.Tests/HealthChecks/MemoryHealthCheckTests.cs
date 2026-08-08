using LastHour.BuildingBlocks.Infrastructure.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.HealthChecks;

public class MemoryHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_BelowMaximum_ReturnsHealthy()
    {
        var check = new MemoryHealthCheck(Options.Create(new HealthChecksOptions
        {
            Memory = new MemoryHealthCheckOptions { MaximumUsedBytes = long.MaxValue },
        }));

        HealthCheckResult result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_AboveMaximum_ReturnsUnhealthy()
    {
        var check = new MemoryHealthCheck(Options.Create(new HealthChecksOptions
        {
            Memory = new MemoryHealthCheckOptions { MaximumUsedBytes = 1 },
        }));

        HealthCheckResult result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }
}
