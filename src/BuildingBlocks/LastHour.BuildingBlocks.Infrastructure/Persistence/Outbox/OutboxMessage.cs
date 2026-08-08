using System.Text.Json;
using LastHour.BuildingBlocks.SharedKernel.Domain.Events;

namespace LastHour.BuildingBlocks.Infrastructure.Persistence.Outbox;

/// <summary>
/// A durable record of a message published through the outbox. Messages are serialized to
/// JSON when written and rehydrated by the outbox processor, which dispatches them to
/// in-process handlers. Processing is at-least-once: a message is retried on subsequent
/// processor cycles until it either succeeds or exhausts the configured retry limit.
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxMessage"/> class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="type">The assembly-qualified CLR type name of the serialized message.</param>
    /// <param name="content">The JSON-serialized message payload.</param>
    /// <param name="occurredOnUtc">The UTC timestamp at which the message was created.</param>
    private OutboxMessage(Guid id, string type, string content, DateTime occurredOnUtc)
    {
        Id = id;
        Type = type;
        Content = content;
        OccurredOnUtc = occurredOnUtc;
    }

    /// <summary>
    /// Gets the message identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the assembly-qualified CLR type name of the serialized message.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Gets the JSON-serialized message payload.
    /// </summary>
    public string Content { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp at which the message was created.
    /// </summary>
    public DateTime OccurredOnUtc { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp at which the message was processed, or <see langword="null"/>
    /// when it is still pending or has exhausted its retries.
    /// </summary>
    public DateTime? ProcessedOnUtc { get; private set; }

    /// <summary>
    /// Gets the last error captured while processing the message, or <see langword="null"/>
    /// when no failure has occurred.
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Gets the number of times the message was processed without success.
    /// </summary>
    public int RetryCount { get; private set; }

    /// <summary>
    /// Creates an outbox message from a domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event to persist.</param>
    /// <returns>A new outbox message ready to be added to the data store.</returns>
    public static OutboxMessage FromDomainEvent(IDomainEvent domainEvent)
        => Create(domainEvent.GetType(), domainEvent);

    /// <summary>
    /// Creates an outbox message from an arbitrary message payload.
    /// </summary>
    /// <param name="message">The message to persist.</param>
    /// <returns>A new outbox message ready to be added to the data store.</returns>
    public static OutboxMessage Create(object message)
        => Create(message.GetType(), message);

    /// <summary>
    /// Marks the message as successfully processed.
    /// </summary>
    /// <param name="processedOnUtc">The UTC timestamp at which processing completed.</param>
    public void MarkProcessed(DateTime processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        Error = null;
    }

    /// <summary>
    /// Records a failed processing attempt. The message remains pending for retry until
    /// <paramref name="maxRetryCount"/> is reached, at which point it is abandoned and
    /// flagged as processed so the retry loop can move on.
    /// </summary>
    /// <param name="error">A description of the failure.</param>
    /// <param name="failedOnUtc">The UTC timestamp at which the attempt failed.</param>
    /// <param name="maxRetryCount">The maximum number of retry attempts allowed.</param>
    public void RecordFailure(string error, DateTime failedOnUtc, int maxRetryCount)
    {
        Error = error;
        RetryCount++;

        if (RetryCount >= maxRetryCount)
        {
            ProcessedOnUtc = failedOnUtc;
        }
    }

    private static OutboxMessage Create(Type messageType, object message)
    {
        string content = JsonSerializer.Serialize(message, messageType, OutboxJson.SerializerOptions);
        return new OutboxMessage(Guid.NewGuid(), messageType.AssemblyQualifiedName ?? messageType.FullName ?? messageType.Name, content, DateTime.UtcNow);
    }
}
