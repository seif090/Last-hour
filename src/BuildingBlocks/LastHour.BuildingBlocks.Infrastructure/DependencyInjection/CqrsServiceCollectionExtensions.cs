using System.Reflection;
using LastHour.BuildingBlocks.Application.Cqrs;
using LastHour.BuildingBlocks.Infrastructure.Exceptions;
using LastHour.BuildingBlocks.Infrastructure.Logging;
using LastHour.BuildingBlocks.Infrastructure.Performance;
using LastHour.BuildingBlocks.Infrastructure.Transactions;
using LastHour.BuildingBlocks.Infrastructure.Validation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LastHour.BuildingBlocks.Infrastructure.DependencyInjection;

/// <summary>
/// Contains extension methods that register the CQRS pipeline with the dependency injection container.
/// </summary>
public static class CqrsServiceCollectionExtensions
{
    /// <summary>
    /// Registers MediatR and automatically scans the Application assembly for command and query
    /// handlers. The method is idempotent: once MediatR is registered, further calls are ignored,
    /// which prevents duplicate handler registrations.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configure">Optional configuration invoked before registration completes; used to
    /// register additional pipeline behaviors such as caching behaviors. Behaviors execute in
    /// registration order (first registered is the outermost wrapper): the always-registered
    /// <see cref="UnhandledExceptionBehavior{TRequest, TResponse}"/>, request logging,
    /// performance and transaction behaviors wrap the user-configured behaviors, which in turn
    /// wrap the <see cref="ValidationBehavior{TRequest, TResponse}"/> that runs immediately before
    /// the handler.</param>
    /// <param name="additionalHandlerAssemblies">Additional assemblies to scan for handlers. The
    /// Application assembly is always scanned and needs not be passed here.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddCqrs(
        this IServiceCollection services,
        Action<MediatRServiceConfiguration>? configure = null,
        params Assembly[] additionalHandlerAssemblies)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (services.Any(descriptor => descriptor.ServiceType == typeof(IMediator)))
        {
            return services;
        }

        services.AddOptions();

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(ICommand).Assembly);
            config.Lifetime = ServiceLifetime.Scoped;

            foreach (Assembly assembly in additionalHandlerAssemblies
                         .Where(assembly => assembly != typeof(ICommand).Assembly)
                         .Distinct())
            {
                config.RegisterServicesFromAssembly(assembly);
            }

            config.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
            config.AddOpenBehavior(typeof(RequestLoggingBehavior<,>));
            config.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            config.AddOpenBehavior(typeof(TransactionBehavior<,>));

            configure?.Invoke(config);

            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }
}
