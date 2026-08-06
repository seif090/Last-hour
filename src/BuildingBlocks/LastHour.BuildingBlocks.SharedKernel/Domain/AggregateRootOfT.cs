namespace LastHour.BuildingBlocks.SharedKernel.Domain;

/// <summary>
/// Provides a common base class for aggregate roots with a strongly typed identifier.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot<TId>
    where TId : IEquatable<TId>
{
}
