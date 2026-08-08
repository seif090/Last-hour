using Microsoft.Extensions.Options;

namespace LastHour.Api.Secrets;

/// <summary>
/// Validates the <see cref="SecretsOptions"/> so misconfiguration fails fast at startup.
/// </summary>
public sealed class SecretsOptionsValidator : IValidateOptions<SecretsOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, SecretsOptions options)
    {
        var failures = new List<string>();

        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrWhiteSpace(options.EnvironmentVariablePrefix))
        {
            failures.Add($"{SecretsOptions.SectionName}:EnvironmentVariablePrefix must not be empty when secrets are enabled.");
        }

        foreach (string secretName in options.Names)
        {
            if (string.IsNullOrWhiteSpace(secretName))
            {
                failures.Add($"{SecretsOptions.SectionName}:Names contains an empty secret name.");
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
