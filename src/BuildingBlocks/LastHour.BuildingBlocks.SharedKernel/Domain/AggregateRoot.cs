namespace LastHour.BuildingBlocks.SharedKernel.Domain;

/// <summary>
/// Provides a common base class for aggregate roots without a strongly typed identifier.
/// </summary>
public abstract class AggregateRoot : Entity, IAggregateRoot
{
}
