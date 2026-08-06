namespace LastHour.BuildingBlocks.Application.Contracts;

/// <summary>
/// Provides access to the tenant of the current request, resolved from the ambient
/// request context without coupling the application layer to a web framework.
/// </summary>
public interface ICurrentTenant
{
    /// <summary>
    /// Gets a value indicating whether a tenant has been resolved for the current request.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Gets the identifier of the current tenant, or <see langword="null"/> when none is resolved.
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Gets the name of the current tenant, or <see langword="null"/> when none is resolved.
    /// </summary>
    string? Name { get; }
}
