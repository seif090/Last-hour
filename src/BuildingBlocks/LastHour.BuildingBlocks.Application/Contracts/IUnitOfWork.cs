namespace LastHour.BuildingBlocks.Application.Contracts;

/// <summary>
/// Defines the persistence boundary that commits the changes tracked by repositories
/// as a single atomic unit, and that exposes explicit transaction control for handlers
/// that must wrap several operations in one database transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Commits all pending changes to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The number of state entries written to the data store.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins an explicit database transaction. Handlers that require multiple operations
    /// to be committed (or rolled back) together must start a transaction before the first
    /// write and must dispose it after <see cref="CommitTransactionAsync"/> or
    /// <see cref="RollbackTransactionAsync"/>.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the explicit transaction started by <see cref="BeginTransactionAsync"/>.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the explicit transaction started by <see cref="BeginTransactionAsync"/>,
    /// discarding every change made within it.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
