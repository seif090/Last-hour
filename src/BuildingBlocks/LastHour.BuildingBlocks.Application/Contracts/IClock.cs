namespace LastHour.BuildingBlocks.Application.Contracts;

/// <summary>
/// Provides the current UTC instant. Implementations wrap a system clock so that time
/// can be controlled in tests and the application never reads <see cref="DateTime.UtcNow"/>
/// directly.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current instant in Coordinated Universal Time (UTC).
    /// </summary>
    DateTime UtcNow { get; }
}
