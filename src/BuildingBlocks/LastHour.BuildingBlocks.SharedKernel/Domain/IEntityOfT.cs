namespace LastHour.BuildingBlocks.SharedKernel.Domain;

/// <summary>
/// Marks a type as an entity that exposes its strongly typed identifier.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public interface IEntity<out TId>
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets the entity identifier.
    /// </summary>
    TId Id { get; }
}
