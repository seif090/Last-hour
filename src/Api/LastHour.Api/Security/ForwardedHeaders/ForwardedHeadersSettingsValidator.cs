using System.Globalization;
using System.Net;
using Microsoft.Extensions.Options;

namespace LastHour.Api.Security.ForwardedHeaders;

/// <summary>
/// Validates the <see cref="ForwardedHeadersSettings"/> so misconfiguration fails fast at startup.
/// Trusting more than the immediate hop without pinning known proxies or networks would let an
/// arbitrary client spoof its address, so that combination is rejected.
/// </summary>
public sealed class ForwardedHeadersSettingsValidator : IValidateOptions<ForwardedHeadersSettings>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, ForwardedHeadersSettings options)
    {
        var failures = new List<string>();

        if (options.ForwardLimit < 1)
        {
            failures.Add($"{ForwardedHeadersSettings.SectionName}:ForwardLimit must be at least 1.");
        }
        else if (options.ForwardLimit > 1 && options.KnownProxies.Length == 0 && options.KnownNetworks.Length == 0)
        {
            failures.Add(
                $"{ForwardedHeadersSettings.SectionName}:ForwardLimit greater than 1 requires KnownProxies or KnownNetworks to be configured.");
        }

        foreach (string proxy in options.KnownProxies)
        {
            if (!IPAddress.TryParse(proxy, out _))
            {
                failures.Add($"{ForwardedHeadersSettings.SectionName}:KnownProxies contains an invalid IP address '{proxy}'.");
            }
        }

        foreach (string network in options.KnownNetworks)
        {
            string[] parts = network.Split('/');
            if (parts.Length != 2
                || !IPAddress.TryParse(parts[0], out _)
                || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int prefixLength)
                || prefixLength is < 0 or > 32)
            {
                failures.Add($"{ForwardedHeadersSettings.SectionName}:KnownNetworks contains an invalid CIDR network '{network}'.");
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
