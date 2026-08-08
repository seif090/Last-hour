using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.HealthChecks;

/// <summary>
/// Reports the health of the drive hosting the configured path based on the free space
/// available, degrading to unhealthy when it falls below the configured minimum.
/// </summary>
public sealed class DiskHealthCheck : IHealthCheck
{
    private readonly IOptions<HealthChecksOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiskHealthCheck"/> class.
    /// </summary>
    /// <param name="options">The health check options.</param>
    public DiskHealthCheck(IOptions<HealthChecksOptions> options)
    {
        _options = options;
    }

    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        HealthChecksOptions options = _options.Value;
        string path = string.IsNullOrWhiteSpace(options.Disk.Path) ? AppContext.BaseDirectory : options.Disk.Path;

        try
        {
            var drive = new DriveInfo(path);
            if (!drive.IsReady)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy($"Disk at '{path}' is not ready."));
            }

            long freeMegabytes = drive.AvailableFreeSpace / (1024L * 1024L);
            long minimumMegabytes = options.Disk.MinimumFreeMegabytes;

            return Task.FromResult(freeMegabytes >= minimumMegabytes
                ? HealthCheckResult.Healthy($"Free disk space: {freeMegabytes} MB.")
                : HealthCheckResult.Unhealthy($"Low disk space: {freeMegabytes} MB free; minimum is {minimumMegabytes} MB."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Disk health check failed.", ex));
        }
    }
}
