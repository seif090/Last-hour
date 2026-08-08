using FluentValidation;
using LastHour.BuildingBlocks.Application.Contracts;
using LastHour.BuildingBlocks.Application.Cqrs;
using LastHour.BuildingBlocks.Infrastructure.DependencyInjection;
using LastHour.BuildingBlocks.Infrastructure.Validation;
using LastHour.BuildingBlocks.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.Validation;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Send_InvalidCommand_ReturnsFailedResultWithoutInvokingHandler()
    {
        using ServiceProvider provider = CreateProvider(
            services => services.AddTransient<IValidator<TestCommand>, TestCommandValidator>());
        IMediator mediator = provider.GetRequiredService<IMediator>();
        TestCommandHandler.Invoked = false;

        Result result = await mediator.Send(new TestCommand(""));

        Assert.False(TestCommandHandler.Invoked);
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.ErrorType);

        ValidationError validationError = Assert.IsType<ValidationError>(result.FirstError);
        Assert.Equal(2, validationError.Validations.Count);
        Assert.Contains(validationError.Validations, error => error.Code == "NotEmptyValidator");
        Assert.Contains(validationError.Validations, error => error.Code == "MustContainPrefix");
    }

    [Fact]
    public async Task Send_ValidCommand_InvokesHandlerAndReturnsSuccess()
    {
        using ServiceProvider provider = CreateProvider(
            services => services.AddTransient<IValidator<TestCommand>, TestCommandValidator>());
        IMediator mediator = provider.GetRequiredService<IMediator>();
        TestCommandHandler.Invoked = false;

        Result result = await mediator.Send(new TestCommand("ok-value"));

        Assert.True(TestCommandHandler.Invoked);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Send_InvalidQuery_ReturnsFailedResultOfT()
    {
        using ServiceProvider provider = CreateProvider(
            services => services.AddTransient<IValidator<TestQuery>, AsyncTestQueryValidator>());
        IMediator mediator = provider.GetRequiredService<IMediator>();
        TestQueryHandler.Invoked = false;

        Result<int> result = await mediator.Send(new TestQuery(-1));

        Assert.False(TestQueryHandler.Invoked);
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.ErrorType);

        ValidationError validationError = Assert.IsType<ValidationError>(result.FirstError);
        Assert.Equal("MustBePositive", Assert.Single(validationError.Validations).Code);
    }

    [Fact]
    public async Task Send_ValidQuery_ReturnsSuccessResultOfT()
    {
        using ServiceProvider provider = CreateProvider(
            services => services.AddTransient<IValidator<TestQuery>, AsyncTestQueryValidator>());
        IMediator mediator = provider.GetRequiredService<IMediator>();
        TestQueryHandler.Invoked = false;

        Result<int> result = await mediator.Send(new TestQuery(7));

        Assert.True(TestQueryHandler.Invoked);
        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value);
    }

    [Fact]
    public async Task Send_WithoutValidators_InvokesHandler()
    {
        using ServiceProvider provider = CreateProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();
        TestCommandHandler.Invoked = false;

        Result result = await mediator.Send(new TestCommand("anything"));

        Assert.True(TestCommandHandler.Invoked);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Send_NonResultRequest_IgnoresValidationBehavior()
    {
        using ServiceProvider provider = CreateProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();

        string result = await mediator.Send(new NonResultRequest("value"));

        Assert.Equal("handled", result);
    }

    [Fact]
    public void AddCqrs_RegistersValidationBehavior()
    {
        var services = new ServiceCollection();

        services.AddCqrs();

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IPipelineBehavior<,>)
                          && descriptor.ImplementationType == typeof(ValidationBehavior<,>));
    }

    private static ServiceProvider CreateProvider(Action<IServiceCollection>? register = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCqrs(null, typeof(TestCommandHandler).Assembly);
        services.AddScoped<IUnitOfWork>(_ => new NoopUnitOfWork());
        register?.Invoke(services);
        return services.BuildServiceProvider();
    }

    private sealed record TestCommand(string Value) : ICommand;

    private sealed class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(command => command.Value)
                .NotEmpty()
                .Must(value => value.Contains('-'))
                .WithErrorCode("MustContainPrefix");
        }
    }

    private sealed class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public static bool Invoked;

        public Task<Result> Handle(TestCommand request, CancellationToken cancellationToken)
        {
            Invoked = true;
            return Task.FromResult(Result.Success());
        }
    }

    private sealed record TestQuery(int Value) : IQuery<int>;

    private sealed class AsyncTestQueryValidator : AbstractValidator<TestQuery>
    {
        public AsyncTestQueryValidator()
        {
            RuleFor(query => query.Value)
                .MustAsync((value, cancellationToken) => Task.FromResult(value > 0))
                .WithErrorCode("MustBePositive");
        }
    }

    private sealed class TestQueryHandler : IQueryHandler<TestQuery, int>
    {
        public static bool Invoked;

        public Task<Result<int>> Handle(TestQuery request, CancellationToken cancellationToken)
        {
            Invoked = true;
            return Task.FromResult(Result<int>.Success(request.Value));
        }
    }

    private sealed record NonResultRequest(string Value) : IRequest<string>;

    private sealed class NonResultRequestHandler : IRequestHandler<NonResultRequest, string>
    {
        public Task<string> Handle(NonResultRequest request, CancellationToken cancellationToken)
            => Task.FromResult("handled");
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
