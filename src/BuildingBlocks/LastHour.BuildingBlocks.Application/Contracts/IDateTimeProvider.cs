namespace LastHour.BuildingBlocks.Application.Contracts;

/// <summary>
/// Provides the current date and time in both local and UTC representations.
/// Implementations abstract the system clock so that time-dependent logic can be tested.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current local date and time.
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Gets the current instant in Coordinated Universal Time (UTC).
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Gets the current instant in Coordinated Universal Time (UTC) with an offset.
    /// </summary>
    DateTimeOffset UtcNowOffset { get; }
}
