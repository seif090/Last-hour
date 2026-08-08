using LastHour.BuildingBlocks.Infrastructure.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.HealthChecks;

public class DiskHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_EnoughFreeSpace_ReturnsHealthy()
    {
        var check = new DiskHealthCheck(Options.Create(new HealthChecksOptions
        {
            Disk = new DiskHealthCheckOptions
            {
                Path = Path.GetTempPath(),
                MinimumFreeMegabytes = 1,
            },
        }));

        HealthCheckResult result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_InsufficientFreeSpace_ReturnsUnhealthy()
    {
        var check = new DiskHealthCheck(Options.Create(new HealthChecksOptions
        {
            Disk = new DiskHealthCheckOptions
            {
                Path = Path.GetTempPath(),
                MinimumFreeMegabytes = int.MaxValue,
            },
        }));

        HealthCheckResult result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_InvalidPath_ReturnsUnhealthy()
    {
        var check = new DiskHealthCheck(Options.Create(new HealthChecksOptions
        {
            Disk = new DiskHealthCheckOptions { Path = "Z:\\does\\not\\exist\\drive" },
        }));

        HealthCheckResult result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }
}
