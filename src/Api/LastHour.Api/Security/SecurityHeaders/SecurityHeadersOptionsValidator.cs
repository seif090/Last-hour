using Microsoft.Extensions.Options;

namespace LastHour.Api.Security.SecurityHeaders;

/// <summary>
/// Validates the <see cref="SecurityHeadersOptions"/> so misconfiguration fails fast at startup.
/// </summary>
public sealed class SecurityHeadersOptionsValidator : IValidateOptions<SecurityHeadersOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, SecurityHeadersOptions options)
    {
        var failures = new List<string>();

        if (options.Hsts.MaxAgeDays <= 0)
        {
            failures.Add($"{SecurityHeadersOptions.SectionName}:Hsts:MaxAgeDays must be a positive number of days.");
        }

        if (string.IsNullOrWhiteSpace(options.ReferrerPolicy))
        {
            failures.Add($"{SecurityHeadersOptions.SectionName}:ReferrerPolicy must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(options.FrameOptions))
        {
            failures.Add($"{SecurityHeadersOptions.SectionName}:FrameOptions must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(options.PermissionsPolicy))
        {
            failures.Add($"{SecurityHeadersOptions.SectionName}:PermissionsPolicy must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(options.ContentSecurityPolicy))
        {
            failures.Add($"{SecurityHeadersOptions.SectionName}:ContentSecurityPolicy must not be empty.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
