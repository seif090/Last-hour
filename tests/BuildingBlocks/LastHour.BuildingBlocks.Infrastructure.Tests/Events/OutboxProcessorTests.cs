using LastHour.BuildingBlocks.Infrastructure.Persistence;
using LastHour.BuildingBlocks.Infrastructure.Persistence.Configuration;
using LastHour.BuildingBlocks.Infrastructure.Persistence.Outbox;
using LastHour.BuildingBlocks.Infrastructure.Tests.Persistence.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.Events;

public class OutboxProcessorTests
{
    [Fact]
    public async Task StartAsync_PendingMessage_DispatchesToHandlersAndMarksProcessed()
    {
        TestSink.Reset();
        string databaseName = Guid.NewGuid().ToString();
        var options = new OutboxOptions { Enabled = true, ProcessingInterval = TimeSpan.FromSeconds(1) };
        using ServiceProvider provider = BuildProcessorProvider(databaseName, options);
        LastHourDbContext context = provider.GetRequiredService<LastHourDbContext>();
        context.OutboxMessages.Add(OutboxMessage.Create(new TestMessage("hello")));
        await context.SaveChangesAsync();

        var processor = provider.GetRequiredService<OutboxProcessor>();
        await processor.StartAsync(CancellationToken.None);
        try
        {
            bool received = await WaitUntilAsync(
                () => TestSink.Messages.Any(message => message is TestMessage),
                TimeSpan.FromSeconds(5));

            Assert.True(received, "The handler did not receive the message.");
        }
        finally
        {
            await processor.StopAsync(CancellationToken.None);
        }

        OutboxMessage stored = await context.OutboxMessages.AsNoTracking().SingleAsync();
        Assert.NotNull(stored.ProcessedOnUtc);
        Assert.Null(stored.Error);
    }

    [Fact]
    public async Task StartAsync_FailingMessage_RecordsErrorAndIsAbandonedAfterMaxRetries()
    {
        TestSink.Reset();
        string databaseName = Guid.NewGuid().ToString();
        var options = new OutboxOptions { Enabled = true, ProcessingInterval = TimeSpan.FromSeconds(1), MaxRetryCount = 1 };
        using ServiceProvider provider = BuildProcessorProvider(databaseName, options);
        LastHourDbContext context = provider.GetRequiredService<LastHourDbContext>();
        context.OutboxMessages.Add(OutboxMessage.Create(new ThrowingTestMessage("boom")));
        await context.SaveChangesAsync();

        var processor = provider.GetRequiredService<OutboxProcessor>();
        await processor.StartAsync(CancellationToken.None);
        try
        {
            bool abandoned = await WaitUntilAsync(
                async () => (await context.OutboxMessages.AsNoTracking().SingleAsync()).ProcessedOnUtc != null,
                TimeSpan.FromSeconds(5));

            Assert.True(abandoned, "The message was not abandoned after exhausting retries.");
        }
        finally
        {
            await processor.StopAsync(CancellationToken.None);
        }

        OutboxMessage stored = await context.OutboxMessages.AsNoTracking().SingleAsync();
        Assert.NotNull(stored.Error);
        Assert.True(stored.RetryCount >= 1);
        Assert.NotNull(stored.ProcessedOnUtc);
    }

    private static ServiceProvider BuildProcessorProvider(string databaseName, OutboxOptions options)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(TestMessageNotificationHandler).Assembly));
        services.AddDbContext<LastHourDbContext>(builder => builder.UseInMemoryDatabase(databaseName));
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<OutboxProcessor>();
        return services.BuildServiceProvider();
    }

    private static async Task<bool> WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        DateTime deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return true;
            }

            await Task.Delay(50);
        }

        return condition();
    }

    private static async Task<bool> WaitUntilAsync(Func<Task<bool>> condition, TimeSpan timeout)
    {
        DateTime deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < deadline)
        {
            if (await condition())
            {
                return true;
            }

            await Task.Delay(50);
        }

        return await condition();
    }
}
