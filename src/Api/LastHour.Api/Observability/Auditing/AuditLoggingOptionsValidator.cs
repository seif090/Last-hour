using Microsoft.Extensions.Options;

namespace LastHour.Api.Observability.Auditing;

/// <summary>
/// Validates the <see cref="AuditLoggingOptions"/> so misconfiguration fails fast at startup.
/// </summary>
public sealed class AuditLoggingOptionsValidator : IValidateOptions<AuditLoggingOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, AuditLoggingOptions options)
    {
        var failures = new List<string>();

        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrWhiteSpace(options.FilePath))
        {
            failures.Add($"{AuditLoggingOptions.SectionName}:FilePath must not be empty when audit logging is enabled.");
        }

        if (options.RetainedFileCount <= 0)
        {
            failures.Add($"{AuditLoggingOptions.SectionName}:RetainedFileCount must be greater than zero.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
