namespace LastHour.BuildingBlocks.Infrastructure.Performance;

/// <summary>
/// Options that configure the <see cref="PerformanceBehavior{TRequest, TResponse}"/>.
/// </summary>
public sealed class PerformanceBehaviorOptions
{
    /// <summary>
    /// The configuration section the options are bound from.
    /// </summary>
    public const string SectionName = "Cqrs:Performance";

    /// <summary>
    /// Gets or sets the execution time above which a request is considered slow.
    /// </summary>
    public TimeSpan SlowRequestThreshold { get; set; } = TimeSpan.FromSeconds(1);
}
