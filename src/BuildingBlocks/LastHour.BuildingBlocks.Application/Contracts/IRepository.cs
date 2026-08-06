using LastHour.BuildingBlocks.SharedKernel.Domain;

namespace LastHour.BuildingBlocks.Application.Contracts;

/// <summary>
/// Defines write operations over an aggregate root. Implementations track mutations made
/// to the aggregate in memory; changes are committed atomically through
/// <see cref="IUnitOfWork.SaveChangesAsync(System.Threading.CancellationToken)"/>.
/// </summary>
/// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
public interface IRepository<TAggregateRoot>
    where TAggregateRoot : IAggregateRoot
{
    /// <summary>
    /// Adds a new aggregate root to the persistence context.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root to add.</param>
    void Add(TAggregateRoot aggregateRoot);

    /// <summary>
    /// Marks an existing aggregate root as modified in the persistence context.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root to update.</param>
    void Update(TAggregateRoot aggregateRoot);

    /// <summary>
    /// Marks an existing aggregate root for removal from the persistence context.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root to remove.</param>
    void Remove(TAggregateRoot aggregateRoot);

    /// <summary>
    /// Marks a collection of aggregate roots for removal from the persistence context.
    /// </summary>
    /// <param name="aggregateRoots">The aggregate roots to remove.</param>
    void RemoveRange(IEnumerable<TAggregateRoot> aggregateRoots);
}
