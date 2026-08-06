namespace LastHour.BuildingBlocks.SharedKernel.Domain.Events;

/// <summary>
/// Represents an event that captures something meaningful that happened within the domain.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the timestamp in UTC at which the event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}
