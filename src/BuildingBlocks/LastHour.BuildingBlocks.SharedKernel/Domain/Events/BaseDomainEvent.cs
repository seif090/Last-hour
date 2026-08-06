namespace LastHour.BuildingBlocks.SharedKernel.Domain.Events;

/// <summary>
/// Provides a base class for domain events with built-in event identity and occurrence timestamp.
/// </summary>
public abstract class BaseDomainEvent : IDomainEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDomainEvent"/> class.
    /// </summary>
    protected BaseDomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }

    /// <inheritdoc/>
    public Guid EventId { get; }

    /// <inheritdoc/>
    public DateTime OccurredOn { get; }
}
