using LastHour.BuildingBlocks.Application.Contracts;

namespace LastHour.BuildingBlocks.Infrastructure.Time;

/// <summary>
/// <see cref="IClock"/> implementation that reads the system clock. Registered as a singleton
/// so consumers can control time in tests by swapping this registration.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc/>
    public DateTime UtcNow => DateTime.UtcNow;
}
