using System.Text.Json;
using LastHour.BuildingBlocks.Infrastructure.Events;
using LastHour.BuildingBlocks.Infrastructure.Persistence;
using LastHour.BuildingBlocks.Infrastructure.Persistence.Outbox;
using LastHour.BuildingBlocks.Infrastructure.Tests.Persistence.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.Events;

public class OutboxPublisherTests
{
    [Fact]
    public async Task PublishAsync_PersistsOutboxMessage()
    {
        using LastHourDbContext context = TestPersistence.CreateContext();
        var publisher = new OutboxPublisher(context);

        await publisher.PublishAsync(new TestMessage("hello"));

        OutboxMessage stored = await context.OutboxMessages.SingleAsync();
        Assert.Contains(nameof(TestMessage), stored.Type);
        Assert.Null(stored.ProcessedOnUtc);

        TestMessage? payload = JsonSerializer.Deserialize<TestMessage>(stored.Content, OutboxJson.SerializerOptions);
        Assert.NotNull(payload);
        Assert.Equal("hello", payload!.Value);
    }

    [Fact]
    public async Task PublishAsync_ObjectOverload_PersistsOutboxMessage()
    {
        using LastHourDbContext context = TestPersistence.CreateContext();
        var publisher = new OutboxPublisher(context);
        object message = new TestMessage("hello");

        await publisher.PublishAsync(message);

        Assert.Equal(1, await context.OutboxMessages.CountAsync());
    }
}
