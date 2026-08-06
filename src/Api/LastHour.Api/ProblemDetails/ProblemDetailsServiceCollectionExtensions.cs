namespace LastHour.Api.ProblemDetails;

/// <summary>
/// Registers the RFC 7807 problem details surface: framework support, the unhandled-exception
/// handler and the reusable components that map SharedKernel results onto HTTP responses.
/// </summary>
public static class ProblemDetailsServiceCollectionExtensions
{
    /// <summary>
    /// Registers problem details support and the unhandled-exception handler.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddLastHourProblemDetails(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddProblemDetails();
        services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

        return services;
    }
}
