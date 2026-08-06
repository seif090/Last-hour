using LastHour.BuildingBlocks.Application.Contracts;
using LastHour.BuildingBlocks.Application.Cqrs;
using LastHour.BuildingBlocks.Infrastructure.DependencyInjection;
using LastHour.BuildingBlocks.Infrastructure.Transactions;
using LastHour.BuildingBlocks.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.Transactions;

public class TransactionBehaviorTests
{
    [Fact]
    public async Task Handle_CommandSuccess_CommitsPendingChanges()
    {
        var unitOfWork = new TrackingUnitOfWork();
        var behavior = new TransactionBehavior<TestCommand, Result>(unitOfWork);

        Result response = await behavior.Handle(
            new TestCommand("value"),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_CommandFailure_DoesNotCommit()
    {
        var unitOfWork = new TrackingUnitOfWork();
        var behavior = new TransactionBehavior<TestCommand, Result>(unitOfWork);

        Result response = await behavior.Handle(
            new TestCommand("value"),
            _ => Task.FromResult(Result.Failure(Error.Validation("Failed", "boom"))),
            CancellationToken.None);

        Assert.True(response.IsFailure);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_ThrowingCommand_DoesNotCommitAndRethrows()
    {
        var unitOfWork = new TrackingUnitOfWork();
        var behavior = new TransactionBehavior<TestCommand, Result>(unitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(
                new TestCommand("value"),
                _ => throw new InvalidOperationException("boom"),
                CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_Query_PassesThroughWithoutCommitting()
    {
        var unitOfWork = new TrackingUnitOfWork();
        var behavior = new TransactionBehavior<TestQuery, Result<int>>(unitOfWork);

        Result<int> response = await behavior.Handle(
            new TestQuery(42),
            _ => Task.FromResult(Result<int>.Success(42)),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(42, response.Value);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public void AddCqrs_RegistersTransactionBehavior()
    {
        var services = new ServiceCollection();

        services.AddCqrs();

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IPipelineBehavior<,>)
                          && descriptor.ImplementationType == typeof(TransactionBehavior<,>));
    }

    [Fact]
    public async Task Send_ThroughPipeline_CommitsSuccessfulCommand()
    {
        var unitOfWork = new TrackingUnitOfWork();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCqrs(null, typeof(TestCommandHandler).Assembly);
        services.AddPerformanceBehaviorOptions(options => options.SlowRequestThreshold = TimeSpan.FromMinutes(1));
        services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        using ServiceProvider provider = services.BuildServiceProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();

        Result result = await mediator.Send(new TestCommand("value"));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Send_ThroughPipeline_DoesNotCommitFailedCommand()
    {
        var unitOfWork = new TrackingUnitOfWork();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCqrs(null, typeof(FailingCommandHandler).Assembly);
        services.AddPerformanceBehaviorOptions(options => options.SlowRequestThreshold = TimeSpan.FromMinutes(1));
        services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        using ServiceProvider provider = services.BuildServiceProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();

        Result result = await mediator.Send(new FailingCommand());

        Assert.True(result.IsFailure);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Send_ThroughPipeline_QueryNeverTouchesUnitOfWork()
    {
        var unitOfWork = new TrackingUnitOfWork();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCqrs(null, typeof(TestQueryHandler).Assembly);
        services.AddPerformanceBehaviorOptions(options => options.SlowRequestThreshold = TimeSpan.FromMinutes(1));
        services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        using ServiceProvider provider = services.BuildServiceProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();

        Result<int> result = await mediator.Send(new TestQuery(42));

        Assert.True(result.IsSuccess);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private sealed record TestCommand(string Value) : ICommand;

    private sealed class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task<Result> Handle(TestCommand request, CancellationToken cancellationToken)
            => Task.FromResult(Result.Success());
    }

    private sealed record FailingCommand : ICommand;

    private sealed class FailingCommandHandler : ICommandHandler<FailingCommand>
    {
        public Task<Result> Handle(FailingCommand request, CancellationToken cancellationToken)
            => Task.FromResult(Result.Failure(Error.Validation("Failed", "boom")));
    }

    private sealed record TestQuery(int Id) : IQuery<int>;

    private sealed class TestQueryHandler : IQueryHandler<TestQuery, int>
    {
        public Task<Result<int>> Handle(TestQuery request, CancellationToken cancellationToken)
            => Task.FromResult(Result<int>.Success(request.Id));
    }

    private sealed class TrackingUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }
}
