using LastHour.BuildingBlocks.Application.Contracts;
using LastHour.BuildingBlocks.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LastHour.BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Maintains the audit trail of <see cref="IAuditableEntity"/> implementations: it stamps
/// created entities with their creation time and actor, and modified entities with the last
/// modification time and actor, at the moment changes are saved.
/// </summary>
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditInterceptor"/> class.
    /// </summary>
    /// <param name="clock">The clock used to timestamp audit events.</param>
    /// <param name="currentUser">The current user, recorded as the audit actor.</param>
    public AuditInterceptor(IClock clock, ICurrentUser currentUser)
    {
        _clock = clock;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (EntityEntry entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is not IAuditableEntity auditable)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                auditable.CreatedAt = _clock.UtcNow;
                auditable.CreatedBy ??= _currentUser.UserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                auditable.UpdatedAt = _clock.UtcNow;
                auditable.UpdatedBy = _currentUser.UserId;
            }
        }
    }
}
