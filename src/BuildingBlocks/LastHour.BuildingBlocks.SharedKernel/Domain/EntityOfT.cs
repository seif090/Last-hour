namespace LastHour.BuildingBlocks.SharedKernel.Domain;

/// <summary>
/// Provides a common base class for domain entities with a strongly typed identifier.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class Entity<TId> : Entity, IEntity<TId>
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets or sets the entity identifier. Assignable only from within the entity or its derived types.
    /// </summary>
    public TId Id { get; protected set; } = default!;
}
