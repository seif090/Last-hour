using LastHour.BuildingBlocks.Application.Contracts;

namespace LastHour.BuildingBlocks.Infrastructure.Time;

/// <summary>
/// Default <see cref="ICurrentTenant"/> used outside an HTTP request context. It represents
/// the absence of a resolved tenant. The API host registers a tenant-aware implementation
/// backed by the current request context.
/// </summary>
public sealed class DefaultCurrentTenant : ICurrentTenant
{
    /// <inheritdoc/>
    public bool IsAvailable => false;

    /// <inheritdoc/>
    public string? TenantId => null;

    /// <inheritdoc/>
    public string? Name => null;
}
