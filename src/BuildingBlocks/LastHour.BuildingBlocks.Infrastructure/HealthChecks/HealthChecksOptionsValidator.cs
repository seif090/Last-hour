using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.HealthChecks;

/// <summary>
/// Validates the <see cref="HealthChecksOptions"/> so misconfiguration fails fast at startup.
/// </summary>
public sealed class HealthChecksOptionsValidator : IValidateOptions<HealthChecksOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, HealthChecksOptions options)
    {
        var errors = new List<string>();

        if (options.TimeoutSeconds <= 0)
        {
            errors.Add("HealthChecks:TimeoutSeconds must be a positive number of seconds.");
        }

        if (options.Disk.MinimumFreeMegabytes < 0)
        {
            errors.Add("HealthChecks:Disk:MinimumFreeMegabytes must not be negative.");
        }

        if (options.Memory.MaximumUsedBytes <= 0)
        {
            errors.Add("HealthChecks:Memory:MaximumUsedBytes must be a positive number of bytes.");
        }

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}
