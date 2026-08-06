using System.Globalization;
using LastHour.Api.Caching.OutputCache;
using LastHour.Api.RateLimiting;
using LastHour.Api.Versioning;

namespace LastHour.Api.Endpoints.System;

/// <summary>
/// Maps the system feature endpoints: the unversioned service root that identifies the running
/// API and the versioned status probe that demonstrates the URL segment versioning contract.
/// Feature endpoint modules follow the same shape: a static class exposing an
/// <see cref="IEndpointRouteBuilder"/> extension method, grouped under
/// <c>LastHour.Api.Endpoints.&lt;Feature&gt;</c>, that only orchestrates the request.
/// </summary>
public static class SystemEndpoints
{
    /// <summary>
    /// Maps the system endpoints onto the route table.
    /// </summary>
    /// <param name="app">The endpoint route builder to extend.</param>
    /// <returns>The same endpoint route builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapGet("/", () => Results.Ok(new ServiceInfo("LastHour.Api", "ready")))
            .CacheOutput(OutputCacheProfile.DefaultName)
            .WithName("Root")
            .WithTags("System");

        app.NewVersionedApi("System")
            .MapGet("api/v{version:apiVersion}/system/status", (HttpContext http) =>
                Results.Ok(new SystemStatus(
                    http.GetRequestedApiVersion()?.ToString("'v'VVV", CultureInfo.InvariantCulture) ?? "unspecified")))
            .HasApiVersion(ApiVersions.V1)
            .RequireRateLimiting(RateLimitPolicyNames.SystemStatus)
            .WithName("SystemStatus")
            .WithTags("System");

        return app;
    }

    /// <summary>
    /// Describes the API service exposed by the root endpoint.
    /// </summary>
    private sealed class ServiceInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceInfo"/> class.
        /// </summary>
        /// <param name="service">The name of the service.</param>
        /// <param name="status">The operational status of the service.</param>
        public ServiceInfo(string service, string status)
        {
            Service = service;
            Status = status;
        }

        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        public string Service { get; }

        /// <summary>
        /// Gets the operational status of the service.
        /// </summary>
        public string Status { get; }
    }

    /// <summary>
    /// Describes the API version served by the versioned status endpoint.
    /// </summary>
    private sealed class SystemStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemStatus"/> class.
        /// </summary>
        /// <param name="version">The API version that served the request.</param>
        public SystemStatus(string version)
        {
            Version = version;
        }

        /// <summary>
        /// Gets the API version that served the request.
        /// </summary>
        public string Version { get; }
    }
}
