using LastHour.BuildingBlocks.Application.Contracts;
using LastHour.BuildingBlocks.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace LastHour.BuildingBlocks.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IRepository{TAggregateRoot}"/>. Repositories
/// only track changes in memory; mutations are committed atomically by the
/// <see cref="IUnitOfWork"/>.
/// </summary>
/// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
public sealed class EfRepository<TAggregateRoot> : IRepository<TAggregateRoot>
    where TAggregateRoot : class, IAggregateRoot
{
    private readonly DbSet<TAggregateRoot> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfRepository{TAggregateRoot}"/> class.
    /// </summary>
    /// <param name="dbContext">The database context that tracks the aggregate roots.</param>
    public EfRepository(LastHourDbContext dbContext)
    {
        _dbSet = dbContext.Set<TAggregateRoot>();
    }

    /// <inheritdoc/>
    public void Add(TAggregateRoot aggregateRoot) => _dbSet.Add(aggregateRoot);

    /// <inheritdoc/>
    public void Update(TAggregateRoot aggregateRoot) => _dbSet.Update(aggregateRoot);

    /// <inheritdoc/>
    public void Remove(TAggregateRoot aggregateRoot) => _dbSet.Remove(aggregateRoot);

    /// <inheritdoc/>
    public void RemoveRange(IEnumerable<TAggregateRoot> aggregateRoots) => _dbSet.RemoveRange(aggregateRoots);
}
