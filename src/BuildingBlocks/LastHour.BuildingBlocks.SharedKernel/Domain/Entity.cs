using LastHour.BuildingBlocks.SharedKernel.Domain.Events;

namespace LastHour.BuildingBlocks.SharedKernel.Domain;

/// <summary>
/// Provides a common base class for domain entities that support raising domain events.
/// </summary>
public abstract class Entity : IEntity
{
    private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

    /// <summary>
    /// Gets the domain events raised by the entity that have not been dispatched yet.
    /// </summary>
    /// <returns>An <see cref="IReadOnlyCollection{T}"/> containing the pending domain events.</returns>
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();

    /// <summary>
    /// Removes all pending domain events from the entity.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Registers a domain event that will be dispatched once the entity changes are persisted.
    /// </summary>
    /// <param name="domainEvent">The domain event to register.</param>
    /// <exception cref="ArgumentNullException"><paramref name="domainEvent"/> is <see langword="null"/>.</exception>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }
}
