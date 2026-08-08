using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.HealthChecks;

/// <summary>
/// Reports the health of the process based on its working set, degrading to unhealthy when
/// the memory footprint exceeds the configured maximum.
/// </summary>
public sealed class MemoryHealthCheck : IHealthCheck
{
    private readonly IOptions<HealthChecksOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryHealthCheck"/> class.
    /// </summary>
    /// <param name="options">The health check options.</param>
    public MemoryHealthCheck(IOptions<HealthChecksOptions> options)
    {
        _options = options;
    }

    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        long workingSetBytes = Environment.WorkingSet;
        long maximumBytes = _options.Value.Memory.MaximumUsedBytes;

        return Task.FromResult(workingSetBytes <= maximumBytes
            ? HealthCheckResult.Healthy($"Working set: {FormatMegabytes(workingSetBytes)}.")
            : HealthCheckResult.Unhealthy($"Working set {FormatMegabytes(workingSetBytes)} exceeds the maximum of {FormatMegabytes(maximumBytes)}."));
    }

    private static string FormatMegabytes(long bytes) => $"{bytes / (1024L * 1024L):N0} MB";
}
