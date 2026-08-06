namespace LastHour.BuildingBlocks.Application.Contracts;

/// <summary>
/// Publishes messages to interested subscribers. Implementations may deliver messages
/// in-process, to an outbox, or to an external message broker; the contract itself is
/// framework-independent.
/// </summary>
public interface IPublisher
{
    /// <summary>
    /// Publishes a message of a known type to its subscribers.
    /// </summary>
    /// <typeparam name="TEvent">The type of the message to publish.</typeparam>
    /// <param name="eventMessage">The message to publish.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that completes when the message has been published.</returns>
    Task PublishAsync<TEvent>(TEvent eventMessage, CancellationToken cancellationToken = default)
        where TEvent : class;

    /// <summary>
    /// Publishes a message whose type is only known at runtime to its subscribers.
    /// </summary>
    /// <param name="eventMessage">The message to publish.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that completes when the message has been published.</returns>
    Task PublishAsync(object eventMessage, CancellationToken cancellationToken = default);
}
