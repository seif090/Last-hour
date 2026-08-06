using LastHour.BuildingBlocks.SharedKernel.Domain.Events;

namespace LastHour.BuildingBlocks.Application.Contracts;

/// <summary>
/// Dispatches domain events to their registered handlers within the current process.
/// Implementations are framework-independent and are typically invoked with the domain
/// events collected from aggregate roots during a unit of work.
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatches a single domain event to its handlers.
    /// </summary>
    /// <param name="domainEvent">The domain event to dispatch.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that completes when all handlers have finished.</returns>
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches a collection of domain events to their handlers.
    /// </summary>
    /// <param name="domainEvents">The domain events to dispatch.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that completes when all handlers have finished.</returns>
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
