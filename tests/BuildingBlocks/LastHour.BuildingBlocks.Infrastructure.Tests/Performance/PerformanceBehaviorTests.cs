using LastHour.BuildingBlocks.Application.Contracts;
using LastHour.BuildingBlocks.Application.Cqrs;
using LastHour.BuildingBlocks.Infrastructure.DependencyInjection;
using LastHour.BuildingBlocks.Infrastructure.Performance;
using LastHour.BuildingBlocks.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.Performance;

public class PerformanceBehaviorTests
{
    [Fact]
    public async Task Handle_FastRequest_DoesNotLogWarning()
    {
        var entries = new List<LogEntry>();
        var behavior = new PerformanceBehavior<TestCommand, Result>(
            new CapturingLogger<PerformanceBehavior<TestCommand, Result>>(entries),
            Options.Create(new PerformanceBehaviorOptions { SlowRequestThreshold = TimeSpan.FromMinutes(1) }));

        Result response = await behavior.Handle(
            new TestCommand("value"),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(entries);
    }

    [Fact]
    public async Task Handle_SlowRequest_LogsWarning()
    {
        var entries = new List<LogEntry>();
        var behavior = new PerformanceBehavior<TestCommand, Result>(
            new CapturingLogger<PerformanceBehavior<TestCommand, Result>>(entries),
            Options.Create(new PerformanceBehaviorOptions { SlowRequestThreshold = TimeSpan.Zero }));

        Result response = await behavior.Handle(
            new TestCommand("value"),
            async _ =>
            {
                await Task.Delay(20, CancellationToken.None);
                return Result.Success();
            },
            CancellationToken.None);

        Assert.True(response.IsSuccess);
        LogEntry entry = Assert.Single(entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Equal(nameof(TestCommand), GetProperty(entry, "RequestName"));
        Assert.True(Assert.IsType<double>(GetProperty(entry, "ExecutionTimeMs")) >= 20);
    }

    [Fact]
    public async Task Handle_SlowThrowingRequest_LogsWarningAndRethrows()
    {
        var entries = new List<LogEntry>();
        var behavior = new PerformanceBehavior<TestCommand, Result>(
            new CapturingLogger<PerformanceBehavior<TestCommand, Result>>(entries),
            Options.Create(new PerformanceBehaviorOptions { SlowRequestThreshold = TimeSpan.Zero }));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(
                new TestCommand("value"),
                async _ =>
                {
                    await Task.Delay(20, CancellationToken.None);
                    throw new InvalidOperationException("boom");
                },
                CancellationToken.None));

        LogEntry entry = Assert.Single(entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
    }

    [Fact]
    public async Task Handle_NonResultRequest_StillMeasures()
    {
        var entries = new List<LogEntry>();
        var behavior = new PerformanceBehavior<NonResultRequest, string>(
            new CapturingLogger<PerformanceBehavior<NonResultRequest, string>>(entries),
            Options.Create(new PerformanceBehaviorOptions { SlowRequestThreshold = TimeSpan.Zero }));

        string response = await behavior.Handle(
            new NonResultRequest("value"),
            _ => Task.FromResult("handled"),
            CancellationToken.None);

        Assert.Equal("handled", response);
        LogEntry entry = Assert.Single(entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Equal(nameof(NonResultRequest), GetProperty(entry, "RequestName"));
    }

    [Fact]
    public void AddCqrs_RegistersPerformanceBehavior()
    {
        var services = new ServiceCollection();

        services.AddCqrs();

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IPipelineBehavior<,>)
                          && descriptor.ImplementationType == typeof(PerformanceBehavior<,>));
    }

    [Fact]
    public void AddPerformanceBehaviorOptions_BindsFromConfiguration()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cqrs:Performance:SlowRequestThreshold"] = "00:00:05",
            })
            .Build();
        var services = new ServiceCollection();

        services.AddPerformanceBehaviorOptions(configuration);
        using ServiceProvider provider = services.BuildServiceProvider();

        PerformanceBehaviorOptions options = provider.GetRequiredService<IOptions<PerformanceBehaviorOptions>>().Value;
        Assert.Equal(TimeSpan.FromSeconds(5), options.SlowRequestThreshold);
    }

    [Fact]
    public async Task Send_ThroughPipeline_LogsWarningForSlowRequest()
    {
        var entries = new List<LogEntry>();
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddProvider(new CapturingLoggerProvider(entries)));
        services.AddCqrs(null, typeof(TestCommandHandler).Assembly);
        services.AddPerformanceBehaviorOptions(options => options.SlowRequestThreshold = TimeSpan.Zero);
        services.AddScoped<IUnitOfWork>(_ => new NoopUnitOfWork());
        using ServiceProvider provider = services.BuildServiceProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();

        Result result = await mediator.Send(new TestCommand("value"));

        Assert.True(result.IsSuccess);
        Assert.Contains(
            entries,
            entry => entry.Level == LogLevel.Warning
                     && GetProperty(entry, "RequestName")?.Equals(nameof(TestCommand)) == true);
    }

    private static object? GetProperty(LogEntry entry, string name)
        => entry.Properties.FirstOrDefault(property => property.Key == name).Value;

    private sealed record TestCommand(string Value) : ICommand;

    private sealed class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public async Task<Result> Handle(TestCommand request, CancellationToken cancellationToken)
        {
            await Task.Delay(20, cancellationToken);
            return Result.Success();
        }
    }

    private sealed record NonResultRequest(string Value) : IRequest<string>;

    private sealed class NoopUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(1);

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed record LogEntry(
        LogLevel Level,
        string Message,
        Exception? Exception,
        IReadOnlyList<KeyValuePair<string, object?>> Properties);

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        private readonly List<LogEntry> _entries;

        public CapturingLoggerProvider(List<LogEntry> entries)
        {
            _entries = entries;
        }

        public ILogger CreateLogger(string categoryName) => new CapturingLogger<CapturingLoggerProvider>(_entries);

        public void Dispose()
        {
        }
    }

    private sealed class CapturingLogger<TCategory> : ILogger<TCategory>
    {
        private readonly List<LogEntry> _entries;

        public CapturingLogger(List<LogEntry> entries)
        {
            _entries = entries;
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var properties = state as IReadOnlyList<KeyValuePair<string, object?>> ?? Array.Empty<KeyValuePair<string, object?>>();
            _entries.Add(new LogEntry(logLevel, formatter(state, exception), exception, properties));
        }
    }
}
