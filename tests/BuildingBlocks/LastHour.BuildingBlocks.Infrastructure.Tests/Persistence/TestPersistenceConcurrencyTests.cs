using LastHour.BuildingBlocks.Infrastructure.Persistence;
using LastHour.BuildingBlocks.Infrastructure.Persistence.Outbox;
using LastHour.BuildingBlocks.Infrastructure.Tests.Persistence.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.Persistence;

public class TestPersistenceConcurrencyTests
{
    [Fact]
    public void CreateContext_AfterDbContextProviderModelBuild_ReturnsCorrectModel()
    {
        using ServiceProvider provider = BuildDbContextProvider();
        using (IServiceScope scope = provider.CreateScope())
        {
            LastHourDbContext noConfigContext = scope.ServiceProvider.GetRequiredService<LastHourDbContext>();
            Assert.NotNull(noConfigContext.Model.FindEntityType(typeof(OutboxMessage)));
        }

        using LastHourDbContext context = TestPersistence.CreateContext();

        Assert.NotNull(context.Model.FindEntityType(typeof(TestAggregate)));
        Assert.NotNull(context.Model.FindEntityType(typeof(TestEntity)));
    }

    [Fact]
    public async Task CreateContext_ConcurrentWithDbContextProvider_ReturnsCorrectModel()
    {
        const int rounds = 20;
        const int contextsPerRound = 32;

        for (int round = 0; round < rounds; round++)
        {
            using ServiceProvider provider = BuildDbContextProvider();

            Task[] tasks = Enumerable.Range(0, contextsPerRound)
                .Select(index =>
                {
                    bool useTestPersistence = index % 4 != 0;
                    return Task.Run(() =>
                    {
                        if (useTestPersistence)
                        {
                            using LastHourDbContext context = TestPersistence.CreateContext();
                            Assert.NotNull(context.Model.FindEntityType(typeof(TestAggregate)));
                            Assert.NotNull(context.Model.FindEntityType(typeof(TestEntity)));
                        }
                        else
                        {
                            using IServiceScope scope = provider.CreateScope();
                            LastHourDbContext context = scope.ServiceProvider.GetRequiredService<LastHourDbContext>();
                            Assert.NotNull(context.Model.FindEntityType(typeof(OutboxMessage)));
                        }
                    });
                })
                .ToArray();

            await Task.WhenAll(tasks);
        }
    }

    private static ServiceProvider BuildDbContextProvider()
    {
        var services = new ServiceCollection();
        services.AddDbContext<LastHourDbContext>(builder => builder.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        return services.BuildServiceProvider();
    }
}
