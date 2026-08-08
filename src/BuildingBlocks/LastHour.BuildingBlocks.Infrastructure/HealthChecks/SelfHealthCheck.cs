using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LastHour.BuildingBlocks.Infrastructure.HealthChecks;

/// <summary>
/// Liveness check that reports the process as healthy as long as it is running. Used by the
/// liveness endpoint, whose predicate only includes the <c>live</c> tag, so Kubernetes and other
/// orchestrators can distinguish "process is up" from "dependencies are reachable".
/// </summary>
public sealed class SelfHealthCheck : IHealthCheck
{
    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy("The process is running."));
    }
}
