using LastHour.BuildingBlocks.Application.Contracts;

namespace LastHour.BuildingBlocks.Infrastructure.Time;

/// <summary>
/// Default <see cref="ICurrentUser"/> used outside an HTTP request context (for example in
/// background workers and the outbox processor). It represents an unauthenticated principal.
/// The API host registers an HTTP-aware implementation backed by the current request context.
/// </summary>
public sealed class DefaultCurrentUser : ICurrentUser
{
    /// <inheritdoc/>
    public bool IsAuthenticated => false;

    /// <inheritdoc/>
    public string? UserId => null;

    /// <inheritdoc/>
    public string? Name => null;

    /// <inheritdoc/>
    public IReadOnlyCollection<string> Roles => Array.Empty<string>();

    /// <inheritdoc/>
    public bool IsInRole(string role) => false;
}
