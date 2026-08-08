using LastHour.BuildingBlocks.SharedKernel.Domain;
using LastHour.BuildingBlocks.SharedKernel.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LastHour.BuildingBlocks.Infrastructure.Persistence.Outbox;

/// <summary>
/// Captures the domain events raised by aggregate roots that are being saved and persists
/// them as <see cref="OutboxMessage"/> rows in the same unit of work, so the events are
/// durable and are dispatched only after the transaction commits.
/// </summary>
public sealed class DomainEventsInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        WriteOutboxMessages(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        WriteOutboxMessages(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void WriteOutboxMessages(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        List<Entity> aggregates = context.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .Select(entry => entry.Entity)
            .OfType<Entity>()
            .Where(entity => entity.GetDomainEvents().Count > 0)
            .ToList();

        if (aggregates.Count == 0)
        {
            return;
        }

        DbSet<OutboxMessage> outbox = context.Set<OutboxMessage>();
        foreach (Entity aggregate in aggregates)
        {
            foreach (IDomainEvent domainEvent in aggregate.GetDomainEvents())
            {
                outbox.Add(OutboxMessage.FromDomainEvent(domainEvent));
            }

            aggregate.ClearDomainEvents();
        }
    }
}
