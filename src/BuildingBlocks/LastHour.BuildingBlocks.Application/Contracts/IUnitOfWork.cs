namespace LastHour.BuildingBlocks.Application.Contracts;

/// <summary>
/// Defines the persistence boundary that commits the changes tracked by repositories
/// as a single atomic unit.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Commits all pending changes to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The number of state entries written to the data store.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
