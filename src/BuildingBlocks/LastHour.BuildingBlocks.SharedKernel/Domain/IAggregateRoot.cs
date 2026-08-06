namespace LastHour.BuildingBlocks.SharedKernel.Domain;

/// <summary>
/// Marks a type as an aggregate root within the domain model. An aggregate root is an
/// entity that is the entry point for the aggregate and guarantees its invariants.
/// </summary>
public interface IAggregateRoot : IEntity
{
}
