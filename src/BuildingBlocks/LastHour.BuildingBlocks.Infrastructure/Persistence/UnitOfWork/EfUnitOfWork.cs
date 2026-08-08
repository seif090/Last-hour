using LastHour.BuildingBlocks.Application.Contracts;
using Microsoft.EntityFrameworkCore.Storage;

namespace LastHour.BuildingBlocks.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// EF Core-backed implementation of <see cref="IUnitOfWork"/>. Committing the unit of work
/// saves every change tracked by the underlying context in a single atomic operation, which
/// includes any outbox messages captured by the domain events interceptor.
/// </summary>
public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly LastHourDbContext _dbContext;
    private IDbContextTransaction? _transaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfUnitOfWork"/> class.
    /// </summary>
    /// <param name="dbContext">The database context whose pending changes are committed.</param>
    public EfUnitOfWork(LastHourDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            throw new InvalidOperationException("A transaction is already in progress for this unit of work.");
        }

        _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction? transaction = _transaction;
        if (transaction is null)
        {
            throw new InvalidOperationException("No transaction has been started for this unit of work.");
        }

        try
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await transaction.DisposeAsync().ConfigureAwait(false);
            _transaction = null;
        }
    }

    /// <inheritdoc/>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction? transaction = _transaction;
        if (transaction is null)
        {
            throw new InvalidOperationException("No transaction has been started for this unit of work.");
        }

        try
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await transaction.DisposeAsync().ConfigureAwait(false);
            _transaction = null;
        }
    }
}
