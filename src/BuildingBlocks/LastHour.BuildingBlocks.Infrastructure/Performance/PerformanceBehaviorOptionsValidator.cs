using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.Performance;

/// <summary>
/// Validates the <see cref="PerformanceBehaviorOptions"/> so misconfiguration fails fast at startup.
/// </summary>
public sealed class PerformanceBehaviorOptionsValidator : IValidateOptions<PerformanceBehaviorOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, PerformanceBehaviorOptions options)
    {
        if (options.SlowRequestThreshold <= TimeSpan.Zero)
        {
            return ValidateOptionsResult.Fail(
                $"'{PerformanceBehaviorOptions.SectionName}:SlowRequestThreshold' must be a positive duration.");
        }

        return ValidateOptionsResult.Success;
    }
}
