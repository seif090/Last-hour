namespace LastHour.BuildingBlocks.SharedKernel.Domain;

/// <summary>
/// Marks a type as an aggregate root that exposes its strongly typed identifier.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root identifier.</typeparam>
public interface IAggregateRoot<out TId> : IAggregateRoot, IEntity<TId>
    where TId : IEquatable<TId>
{
}
