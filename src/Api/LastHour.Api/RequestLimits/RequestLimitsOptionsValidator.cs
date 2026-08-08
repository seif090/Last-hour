using Microsoft.Extensions.Options;

namespace LastHour.Api.RequestLimits;

/// <summary>
/// Validates the <see cref="RequestLimitsOptions"/> so misconfiguration fails fast at startup.
/// </summary>
public sealed class RequestLimitsOptionsValidator : IValidateOptions<RequestLimitsOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, RequestLimitsOptions options)
    {
        var failures = new List<string>();

        if (options.MaxRequestBodySize is long maxBody && maxBody <= 0)
        {
            failures.Add($"{RequestLimitsOptions.SectionName}:MaxRequestBodySize must be greater than zero.");
        }

        if (options.RequestHeadersTimeout is TimeSpan headers && headers <= TimeSpan.Zero)
        {
            failures.Add($"{RequestLimitsOptions.SectionName}:RequestHeadersTimeout must be a positive duration.");
        }

        if (options.KeepAliveTimeout is TimeSpan keepAlive && keepAlive <= TimeSpan.Zero)
        {
            failures.Add($"{RequestLimitsOptions.SectionName}:KeepAliveTimeout must be a positive duration.");
        }

        if (options.MultipartBodyLengthLimit is long multipart && multipart <= 0)
        {
            failures.Add($"{RequestLimitsOptions.SectionName}:MultipartBodyLengthLimit must be greater than zero.");
        }

        if (options.MinRequestBodyDataRateBytesPerSecond is double rate && rate <= 0)
        {
            failures.Add($"{RequestLimitsOptions.SectionName}:MinRequestBodyDataRateBytesPerSecond must be greater than zero.");
        }

        if (options.MinRequestBodyDataRateGracePeriod is TimeSpan grace && grace <= TimeSpan.Zero)
        {
            failures.Add($"{RequestLimitsOptions.SectionName}:MinRequestBodyDataRateGracePeriod must be a positive duration.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
