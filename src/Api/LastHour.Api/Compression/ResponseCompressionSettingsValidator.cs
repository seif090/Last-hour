using Microsoft.Extensions.Options;

namespace LastHour.Api.Compression;

/// <summary>
/// Validates the <see cref="ResponseCompressionSettings"/> so misconfiguration fails fast at startup.
/// </summary>
public sealed class ResponseCompressionSettingsValidator : IValidateOptions<ResponseCompressionSettings>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, ResponseCompressionSettings options)
    {
        if (!Enum.IsDefined(options.CompressionLevel))
        {
            return ValidateOptionsResult.Fail(
                $"'{ResponseCompressionSettings.SectionName}:CompressionLevel' is not a valid compression level.");
        }

        return ValidateOptionsResult.Success;
    }
}
