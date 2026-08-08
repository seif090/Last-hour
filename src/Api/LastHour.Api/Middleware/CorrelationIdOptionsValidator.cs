using Microsoft.Extensions.Options;

namespace LastHour.Api.Middleware;

/// <summary>
/// Validates the <see cref="CorrelationIdOptions"/> so misconfiguration fails fast at startup.
/// </summary>
public sealed class CorrelationIdOptionsValidator : IValidateOptions<CorrelationIdOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, CorrelationIdOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.HeaderName))
        {
            failures.Add($"{CorrelationIdOptions.SectionName}:HeaderName must not be empty.");
        }

        if (options.MaximumIncomingLength <= 0)
        {
            failures.Add($"{CorrelationIdOptions.SectionName}:MaximumIncomingLength must be a positive number.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
