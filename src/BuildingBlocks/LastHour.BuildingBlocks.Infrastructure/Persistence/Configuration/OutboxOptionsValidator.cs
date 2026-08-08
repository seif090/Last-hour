using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.Persistence.Configuration;

/// <summary>
/// Validates <see cref="OutboxOptions"/> so misconfiguration fails fast at startup.
/// </summary>
public sealed class OutboxOptionsValidator : IValidateOptions<OutboxOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, OutboxOptions options)
    {
        if (options.ProcessingInterval < TimeSpan.FromSeconds(1))
        {
            return ValidateOptionsResult.Fail(
                $"'{OutboxOptions.SectionName}:ProcessingInterval' must be at least one second.");
        }

        if (options.BatchSize <= 0)
        {
            return ValidateOptionsResult.Fail(
                $"'{OutboxOptions.SectionName}:BatchSize' must be greater than zero.");
        }

        if (options.MaxRetryCount <= 0)
        {
            return ValidateOptionsResult.Fail(
                $"'{OutboxOptions.SectionName}:MaxRetryCount' must be greater than zero.");
        }

        return ValidateOptionsResult.Success;
    }
}
