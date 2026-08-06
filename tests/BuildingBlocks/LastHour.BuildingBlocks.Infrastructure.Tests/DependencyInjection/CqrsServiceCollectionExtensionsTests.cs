using LastHour.BuildingBlocks.Application.Contracts;
using LastHour.BuildingBlocks.Application.Cqrs;
using LastHour.BuildingBlocks.Infrastructure.DependencyInjection;
using LastHour.BuildingBlocks.Infrastructure.Exceptions;
using LastHour.BuildingBlocks.Infrastructure.Logging;
using LastHour.BuildingBlocks.Infrastructure.Performance;
using LastHour.BuildingBlocks.Infrastructure.Transactions;
using LastHour.BuildingBlocks.Infrastructure.Validation;
using LastHour.BuildingBlocks.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.DependencyInjection;

public class CqrsServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCqrs_RegistersMediator()
    {
        var services = new ServiceCollection();

        services.AddCqrs();
        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IMediator>());
        Assert.NotNull(provider.GetRequiredService<ISender>());
        Assert.NotNull(provider.GetRequiredService<MediatR.IPublisher>());
    }

    [Fact]
    public void AddCqrs_CalledTwice_DoesNotDuplicateRegistrations()
    {
        var services = new ServiceCollection();

        services.AddCqrs();
        services.AddCqrs();

        Assert.Equal(1, services.Count(descriptor => descriptor.ServiceType == typeof(IMediator)));
        Assert.Equal(5, services.Count(descriptor => descriptor.ServiceType == typeof(IPipelineBehavior<,>)));
    }

    [Fact]
    public void AddCqrs_ResolvesPipelineBehaviorsInExecutionOrder()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCqrs(null, typeof(TestCommandHandler).Assembly);
        services.AddPerformanceBehaviorOptions(options => options.SlowRequestThreshold = TimeSpan.FromMinutes(1));
        services.AddScoped<IUnitOfWork>(_ => new NoopUnitOfWork());
        using ServiceProvider provider = services.BuildServiceProvider();

        IPipelineBehavior<TestCommand, Result>[] behaviors = provider
            .GetServices<IPipelineBehavior<TestCommand, Result>>()
            .ToArray();

        Assert.Equal(
            new[]
            {
                typeof(UnhandledExceptionBehavior<TestCommand, Result>),
                typeof(RequestLoggingBehavior<TestCommand, Result>),
                typeof(PerformanceBehavior<TestCommand, Result>),
                typeof(TransactionBehavior<TestCommand, Result>),
                typeof(ValidationBehavior<TestCommand, Result>),
            },
            behaviors.Select(behavior => behavior.GetType()));
    }

    [Fact]
    public void AddCqrs_WithConfigure_RegistersPipelineBehavior()
    {
        var services = new ServiceCollection();

        services.AddCqrs(config => config.AddOpenBehavior(typeof(TestPipelineBehavior<,>)));

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IPipelineBehavior<,>));
    }

    private sealed class TestPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
            => next(cancellationToken);
    }

    private sealed record TestCommand(string Value) : ICommand;

    private sealed class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task<Result> Handle(TestCommand request, CancellationToken cancellationToken)
            => Task.FromResult(Result.Success());
    }

    private sealed class NoopUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(1);
    }
}
