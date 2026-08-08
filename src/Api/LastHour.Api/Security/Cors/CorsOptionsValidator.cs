using Microsoft.Extensions.Options;

namespace LastHour.Api.Security.Cors;

/// <summary>
/// Validates the <see cref="CorsOptions"/> so misconfiguration fails fast at startup. The rules
/// harden the production surface: any-origin access is forbidden outside development, credentials
/// can never be combined with any-origin access (the specification forbids it), and the configured
/// origins must be valid absolute HTTP(S) URIs with at most a leading subdomain wildcard.
/// </summary>
public sealed class CorsOptionsValidator : IValidateOptions<CorsOptions>
{
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorsOptionsValidator"/> class.
    /// </summary>
    /// <param name="environment">The hosting environment, used to tell development from production.</param>
    /// <exception cref="ArgumentNullException"><paramref name="environment"/> is <see langword="null"/>.</exception>
    public CorsOptionsValidator(IHostEnvironment environment)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, CorsOptions options)
    {
        var failures = new List<string>();

        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (options.AllowAnyOrigin)
        {
            if (_environment.IsProduction())
            {
                failures.Add($"{CorsOptions.SectionName}:AllowAnyOrigin is forbidden in the production environment.");
            }

            if (options.AllowCredentials)
            {
                failures.Add(
                    $"{CorsOptions.SectionName}:AllowCredentials cannot be combined with AllowAnyOrigin; credentials are undefined for wildcard origins.");
            }
        }

        foreach (string origin in options.AllowedOrigins)
        {
            if (origin == "*")
            {
                failures.Add($"{CorsOptions.SectionName}:AllowedOrigins must not contain '*'; use AllowAnyOrigin instead.");
                continue;
            }

            string candidate = origin;
            if (origin.StartsWith("https://*.", StringComparison.Ordinal))
            {
                candidate = string.Concat("https://", origin.AsSpan("https://*.".Length));
            }
            else if (origin.StartsWith("http://*.", StringComparison.Ordinal))
            {
                candidate = string.Concat("http://", origin.AsSpan("http://*.".Length));
            }

            if (candidate.Contains('*')
                || !Uri.TryCreate(candidate, UriKind.Absolute, out Uri? uri)
                || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            {
                failures.Add($"{CorsOptions.SectionName}:AllowedOrigins contains an invalid origin '{origin}'.");
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
