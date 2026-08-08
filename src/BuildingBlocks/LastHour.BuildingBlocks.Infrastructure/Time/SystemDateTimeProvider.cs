using LastHour.BuildingBlocks.Application.Contracts;

namespace LastHour.BuildingBlocks.Infrastructure.Time;

/// <summary>
/// <see cref="IDateTimeProvider"/> implementation that reads the system clock in both local
/// and UTC representations.
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc/>
    public DateTime Now => DateTime.Now;

    /// <inheritdoc/>
    public DateTime UtcNow => DateTime.UtcNow;

    /// <inheritdoc/>
    public DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;
}
