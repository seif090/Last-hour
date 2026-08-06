namespace LastHour.BuildingBlocks.SharedKernel.Domain;

/// <summary>
/// Represents a business rule that can be validated within the domain layer.
/// </summary>
public interface IBusinessRule
{
    /// <summary>
    /// Gets a value indicating whether the business rule is satisfied.
    /// </summary>
    bool IsSatisfied { get; }

    /// <summary>
    /// Gets the error message describing why the business rule was not satisfied.
    /// </summary>
    string Message { get; }
}
