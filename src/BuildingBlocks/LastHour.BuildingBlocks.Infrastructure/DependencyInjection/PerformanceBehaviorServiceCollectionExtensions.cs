using LastHour.BuildingBlocks.Infrastructure.Performance;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LastHour.BuildingBlocks.Infrastructure.DependencyInjection;

/// <summary>
/// Contains extension methods that configure the <see cref="PerformanceBehavior{TRequest, TResponse}"/> options.
/// </summary>
public static class PerformanceBehaviorServiceCollectionExtensions
{
    /// <summary>
    /// Binds the performance behavior options from the <see cref="PerformanceBehaviorOptions.SectionName"/> configuration section.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The configuration to bind the options from.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddPerformanceBehaviorOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        return services.Configure<PerformanceBehaviorOptions>(configuration.GetSection(PerformanceBehaviorOptions.SectionName));
    }

    /// <summary>
    /// Configures the performance behavior options programmatically.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configure">The action that configures the options.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configure"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddPerformanceBehaviorOptions(
        this IServiceCollection services,
        Action<PerformanceBehaviorOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        return services.Configure(configure);
    }
}
