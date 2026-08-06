using System.Diagnostics;
using LastHour.BuildingBlocks.Application.Contracts;
using LastHour.BuildingBlocks.Application.Cqrs;
using LastHour.BuildingBlocks.Infrastructure.DependencyInjection;
using LastHour.BuildingBlocks.Infrastructure.Logging;
using LastHour.BuildingBlocks.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.Logging;

public class RequestLoggingBehaviorTests
{
    private static readonly string[] NotEmptyErrorCodes = ["NotEmpty"];

    [Fact]
    public async Task Handle_SuccessfulResult_LogsInformationWithRequestNameAndDuration()
    {
        var entries = new List<LogEntry>();
        var behavior = new RequestLoggingBehavior<TestCommand, Result>(new CapturingLogger<RequestLoggingBehavior<TestCommand, Result>>(entries));

        Result response = await behavior.Handle(
            new TestCommand("value"),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
        LogEntry entry = Assert.Single(entries);
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.Equal(nameof(TestCommand), GetProperty(entry, "RequestName"));
        Assert.True(Assert.IsType<double>(GetProperty(entry, "ExecutionTimeMs")) >= 0);
        Assert.Null(GetProperty(entry, "CorrelationId"));
    }

    [Fact]
    public async Task Handle_WithActiveActivity_LogsCorrelationId()
    {
        var entries = new List<LogEntry>();
        var behavior = new RequestLoggingBehavior<TestCommand, Result>(new CapturingLogger<RequestLoggingBehavior<TestCommand, Result>>(entries));

        using Activity activity = new("TestActivity");
        activity.Start();

        Result response = await behavior.Handle(
            new TestCommand("value"),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
        LogEntry entry = Assert.Single(entries);
        Assert.Equal(activity.TraceId.ToString(), GetProperty(entry, "CorrelationId"));
    }

    [Fact]
    public async Task Handle_FailedResult_LogsWarningWithErrorDetails()
    {
        var entries = new List<LogEntry>();
        var behavior = new RequestLoggingBehavior<TestCommand, Result>(new CapturingLogger<RequestLoggingBehavior<TestCommand, Result>>(entries));

        Result response = await behavior.Handle(
            new TestCommand("value"),
            _ => Task.FromResult<Result>(Result.Failure(Error.Validation("NotEmpty", "'Value' must not be empty."))),
            CancellationToken.None);

        Assert.True(response.IsFailure);
        LogEntry entry = Assert.Single(entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Equal(ErrorType.Validation, GetProperty(entry, "ErrorType"));
        Assert.Equal(NotEmptyErrorCodes, Assert.IsType<string[]>(GetProperty(entry, "ErrorCodes")));
    }

    [Fact]
    public async Task Handle_ThrowingHandler_LogsErrorAndRethrows()
    {
        var entries = new List<LogEntry>();
        var behavior = new RequestLoggingBehavior<TestCommand, Result>(new CapturingLogger<RequestLoggingBehavior<TestCommand, Result>>(entries));
        var exception = new InvalidOperationException("boom");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(
                new TestCommand("value"),
                _ => Task.FromException<Result>(exception),
                CancellationToken.None));

        LogEntry entry = Assert.Single(entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Same(exception, entry.Exception);
    }

    [Fact]
    public void AddCqrs_RegistersRequestLoggingBehavior()
    {
        var services = new ServiceCollection();

        services.AddCqrs();

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IPipelineBehavior<,>)
                          && descriptor.ImplementationType == typeof(RequestLoggingBehavior<,>));
    }

    [Fact]
    public async Task Send_ThroughPipeline_LogsRequest()
    {
        var entries = new List<LogEntry>();
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddProvider(new CapturingLoggerProvider(entries)));
        services.AddCqrs(null, typeof(TestCommandHandler).Assembly);
        services.AddScoped<IUnitOfWork>(_ => new NoopUnitOfWork());
        using ServiceProvider provider = services.BuildServiceProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();
        TestCommandHandler.Invoked = false;

        Result result = await mediator.Send(new TestCommand("value"));

        Assert.True(result.IsSuccess);
        Assert.True(TestCommandHandler.Invoked);
        Assert.Contains(
            entries,
            entry => entry.Level == LogLevel.Information
                     && GetProperty(entry, "RequestName")?.Equals(nameof(TestCommand)) == true);
    }

    private static object? GetProperty(LogEntry entry, string name)
        => entry.Properties.FirstOrDefault(property => property.Key == name).Value;

    private sealed record TestCommand(string Value) : ICommand;

    private sealed class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public static bool Invoked;

        public Task<Result> Handle(TestCommand request, CancellationToken cancellationToken)
        {
            Invoked = true;
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class NoopUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(1);
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
