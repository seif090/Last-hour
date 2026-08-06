using Asp.Versioning;
using Microsoft.OpenApi.Models;

namespace LastHour.Api.OpenApi;

/// <summary>
/// Builds the OpenAPI document metadata (title, description, contact and licensing) shown in the
/// Swagger UI for each API version.
/// </summary>
public static class SwaggerInfoFactory
{
    /// <summary>
    /// Creates the OpenAPI information block for an API version document.
    /// </summary>
    /// <param name="version">The API version the document serves.</param>
    /// <returns>The OpenAPI information block.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="version"/> is <see langword="null"/>.</exception>
    public static OpenApiInfo Create(ApiVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);

        return new OpenApiInfo
        {
            Title = "LastHour API",
            Version = $"v{version.MajorVersion}",
            Description = "The LastHour commerce platform HTTP API. Endpoints are versioned through the " +
                          "URL segment (for example /api/v1/...), every response carries an X-Correlation-ID " +
                          "header that ties the request to its server-side log trail, and error responses " +
                          "follow RFC 7807 problem details.",
            TermsOfService = new Uri("https://lasthour.dev/terms"),
            Contact = new OpenApiContact
            {
                Name = "LastHour Platform Engineering",
                Url = new Uri("https://lasthour.dev/support"),
            },
            License = new OpenApiLicense
            {
                Name = "Proprietary",
                Url = new Uri("https://lasthour.dev/license"),
            },
        };
    }
}
