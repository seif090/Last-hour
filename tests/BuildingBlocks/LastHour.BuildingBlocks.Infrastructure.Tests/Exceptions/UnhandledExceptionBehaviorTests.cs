using LastHour.BuildingBlocks.Application.Contracts;
using LastHour.BuildingBlocks.Application.Cqrs;
using LastHour.BuildingBlocks.Infrastructure.DependencyInjection;
using LastHour.BuildingBlocks.Infrastructure.Exceptions;
using LastHour.BuildingBlocks.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.Exceptions;

public class UnhandledExceptionBehaviorTests
{
    [Fact]
    public async Task Handle_CommandThrows_ReturnsFailedResultAndLogsException()
    {
        var entries = new List<LogEntry>();
        var behavior = new UnhandledExceptionBehavior<TestCommand, Result>(
            new CapturingLogger<UnhandledExceptionBehavior<TestCommand, Result>>(entries));

        Result response = await behavior.Handle(
            new TestCommand("value"),
            _ => throw new InvalidOperationException("boom"),
            CancellationToken.None);

        Assert.True(response.IsFailure);
        Assert.Equal(ErrorType.Failure, response.ErrorType);
        Assert.Equal("UnhandledException", response.FirstError?.Code);
        Assert.Equal("boom", response.FirstError?.Description);

        LogEntry entry = Assert.Single(entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Equal(nameof(TestCommand), GetProperty(entry, "RequestName"));
        Assert.Equal("boom", entry.Exception?.Message);
        Assert.Contains("at ", entry.Exception?.StackTrace);
    }

    [Fact]
    public async Task Handle_QueryThrows_ReturnsFailedResultOfT()
    {
        var entries = new List<LogEntry>();
        var behavior = new UnhandledExceptionBehavior<TestQuery, Result<int>>(
            new CapturingLogger<UnhandledExceptionBehavior<TestQuery, Result<int>>>(entries));

        Result<int> response = await behavior.Handle(
            new TestQuery(42),
            _ => throw new InvalidOperationException("boom"),
            CancellationToken.None);

        Assert.True(response.IsFailure);
        Assert.Equal(ErrorType.Failure, response.ErrorType);
        Assert.Equal("UnhandledException", response.FirstError?.Code);
        Assert.Equal(LogLevel.Error, Assert.Single(entries).Level);
    }

    [Fact]
    public async Task Handle_Success_ReturnsResponseWithoutLogging()
    {
        var entries = new List<LogEntry>();
        var behavior = new UnhandledExceptionBehavior<TestCommand, Result>(
            new CapturingLogger<UnhandledExceptionBehavior<TestCommand, Result>>(entries));

        Result response = await behavior.Handle(
            new TestCommand("value"),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(entries);
    }

    [Fact]
    public async Task Handle_CriticalException_LogsAndRethrows()
    {
        var entries = new List<LogEntry>();
        var behavior = new UnhandledExceptionBehavior<TestCommand, Result>(
            new CapturingLogger<UnhandledExceptionBehavior<TestCommand, Result>>(entries));

        await Assert.ThrowsAsync<InvalidProgramException>(
            () => behavior.Handle(
                new TestCommand("value"),
                _ => throw new InvalidProgramException("critical"),
                CancellationToken.None));

        LogEntry entry = Assert.Single(entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.IsType<InvalidProgramException>(entry.Exception);
    }

    [Fact]
    public async Task Handle_Cancellation_Rethrows()
    {
        var entries = new List<LogEntry>();
        var behavior = new UnhandledExceptionBehavior<TestCommand, Result>(
            new CapturingLogger<UnhandledExceptionBehavior<TestCommand, Result>>(entries));

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => behavior.Handle(
                new TestCommand("value"),
                _ => throw new OperationCanceledException(),
                CancellationToken.None));

        Assert.Single(entries);
    }

    [Fact]
    public void AddCqrs_RegistersUnhandledExceptionBehavior()
    {
        var services = new ServiceCollection();

        services.AddCqrs();

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IPipelineBehavior<,>)
                          && descriptor.ImplementationType == typeof(UnhandledExceptionBehavior<,>));
    }

    [Fact]
    public async Task Send_ThroughPipeline_ConvertsThrowingCommandToFailedResult()
    {
        var entries = new List<LogEntry>();
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddProvider(new CapturingLoggerProvider(entries)));
        services.AddCqrs(null, typeof(ThrowingCommandHandler).Assembly);
        services.AddPerformanceBehaviorOptions(options => options.SlowRequestThreshold = TimeSpan.FromMinutes(1));
        services.AddScoped<IUnitOfWork>(_ => new NoopUnitOfWork());
        using ServiceProvider provider = services.BuildServiceProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();

        Result result = await mediator.Send(new ThrowingCommand());

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Failure, result.ErrorType);
        Assert.Equal("UnhandledException", result.FirstError?.Code);
        Assert.Contains(
            entries,
            entry => entry.Level == LogLevel.Error && entry.Exception?.Message == "boom");
    }

    private static object? GetProperty(LogEntry entry, string name)
        => entry.Properties.FirstOrDefault(property => property.Key == name).Value;

    private sealed record TestCommand(string Value) : ICommand;

    private sealed record TestQuery(int Value) : IQuery<int>;

    private sealed record ThrowingCommand : ICommand;

    private sealed class ThrowingCommandHandler : ICommandHandler<ThrowingCommand>
    {
        public Task<Result> Handle(ThrowingCommand request, CancellationToken cancellationToken)
            => throw new InvalidOperationException("boom");
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
}
