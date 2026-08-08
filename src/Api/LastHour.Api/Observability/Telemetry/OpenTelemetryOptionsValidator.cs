using Microsoft.Extensions.Options;

namespace LastHour.Api.Observability.Telemetry;

/// <summary>
/// Validates the <see cref="OpenTelemetryOptions"/> so misconfiguration fails fast at startup.
/// </summary>
public sealed class OpenTelemetryOptionsValidator : IValidateOptions<OpenTelemetryOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, OpenTelemetryOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ServiceName))
        {
            failures.Add($"{OpenTelemetryOptions.SectionName}:ServiceName must not be empty.");
        }

        if (options.UseOtlpExporter && string.IsNullOrWhiteSpace(options.OtlpEndpoint))
        {
            failures.Add($"{OpenTelemetryOptions.SectionName}:OtlpEndpoint must be configured when the OTLP exporter is enabled.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
