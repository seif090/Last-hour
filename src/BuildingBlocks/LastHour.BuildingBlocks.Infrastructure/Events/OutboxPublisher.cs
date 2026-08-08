using LastHour.BuildingBlocks.Application.Contracts;
using LastHour.BuildingBlocks.Infrastructure.Persistence;
using LastHour.BuildingBlocks.Infrastructure.Persistence.Outbox;

namespace LastHour.BuildingBlocks.Infrastructure.Events;

/// <summary>
/// Outbox-backed <see cref="IPublisher"/>: published messages are serialized and persisted as
/// <see cref="OutboxMessage"/> rows, then dispatched asynchronously by the outbox processor.
/// Persisting the message guarantees delivery is never lost when the current unit of work
/// commits successfully.
/// </summary>
public sealed class OutboxPublisher : IPublisher
{
    private readonly LastHourDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxPublisher"/> class.
    /// </summary>
    /// <param name="dbContext">The database context the outbox rows are written through.</param>
    public OutboxPublisher(LastHourDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task PublishAsync<TEvent>(TEvent eventMessage, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        _dbContext.OutboxMessages.Add(OutboxMessage.Create(eventMessage));
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task PublishAsync(object eventMessage, CancellationToken cancellationToken = default)
    {
        _dbContext.OutboxMessages.Add(OutboxMessage.Create(eventMessage));
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
