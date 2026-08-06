using LastHour.Api.Caching.OutputCache;
using LastHour.Api.Compression;
using LastHour.Api.Middleware;
using LastHour.Api.OpenApi;
using LastHour.Api.ProblemDetails;
using LastHour.Api.RateLimiting;
using LastHour.Api.Versioning;
using LastHour.BuildingBlocks.Infrastructure.DependencyInjection;

namespace LastHour.Api.DependencyInjection;

/// <summary>
/// Registers the LastHour API surface. The API is the composition root: it wires the CQRS
/// pipeline and its options together with the HTTP surface. Infrastructure is referenced
/// only here, at the composition root; feature endpoints orchestrate requests purely through
/// application abstractions and never touch infrastructure types.
/// </summary>
public static class ApiServiceCollectionExtensions
{
    /// <summary>
    /// Registers MVC controllers, API versioning with per-version Swagger documents, health
    /// checks and the CQRS pipeline with its options.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The application configuration used to bind pipeline options.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/>
    /// is <see langword="null"/>.</exception>
    public static IServiceCollection AddLastHourApi(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddHealthChecks();

        services.AddCqrs();
        services.AddPerformanceBehaviorOptions(configuration);

        services.AddLastHourApiVersioning();
        services.AddLastHourSwagger();
        services.AddLastHourProblemDetails();
        services.AddLastHourResponseCompression(configuration);
        services.AddLastHourOutputCache(configuration);
        services.AddLastHourRateLimiting(configuration);
        services.AddLastHourMiddleware();

        return services;
    }
}
