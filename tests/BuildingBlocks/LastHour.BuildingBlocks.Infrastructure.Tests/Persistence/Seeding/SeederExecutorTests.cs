using LastHour.BuildingBlocks.Infrastructure.Persistence;
using LastHour.BuildingBlocks.Infrastructure.Persistence.Seeding;
using LastHour.BuildingBlocks.Infrastructure.Tests.Persistence.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.Persistence.Seeding;

public class SeederExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_FirstRun_RunsSeederAndRecordsHistory()
    {
        using LastHourDbContext context = TestPersistence.CreateContext();
        var executor = new SeederExecutor(context, new FixedClock(), NullLogger<SeederExecutor>.Instance);
        var seeder = new TrackingSeeder();

        await executor.ExecuteAsync(new[] { seeder });

        Assert.Equal(1, seeder.SeedCallCount);

        SeedHistory history = context.Set<SeedHistory>().Single();
        Assert.Equal(nameof(TrackingSeeder), history.SeederType);
        Assert.Equal(TestPersistence.FixedUtcNow, history.ExecutedAtUtc);
    }

    [Fact]
    public async Task ExecuteAsync_SecondRun_SkipsSeederAndKeepsSingleHistoryRecord()
    {
        using LastHourDbContext context = TestPersistence.CreateContext();
        var executor = new SeederExecutor(context, new FixedClock(), NullLogger<SeederExecutor>.Instance);
        var seeder = new TrackingSeeder();

        await executor.ExecuteAsync(new[] { seeder });
        await executor.ExecuteAsync(new[] { seeder });

        Assert.Equal(1, seeder.SeedCallCount);
        Assert.Equal(1, await context.Set<SeedHistory>().CountAsync());
    }

    [Fact]
    public async Task ExecuteAsync_DistinctSeeders_RunsAndRecordsEachOnce()
    {
        using LastHourDbContext context = TestPersistence.CreateContext();
        var executor = new SeederExecutor(context, new FixedClock(), NullLogger<SeederExecutor>.Instance);
        var first = new TrackingSeeder();
        var second = new SecondTrackingSeeder();

        await executor.ExecuteAsync(new IDatabaseSeeder[] { first, second });
        await executor.ExecuteAsync(new IDatabaseSeeder[] { first, second });

        Assert.Equal(1, first.SeedCallCount);
        Assert.Equal(1, second.SeedCallCount);
        Assert.Equal(2, await context.Set<SeedHistory>().CountAsync());
    }

    private sealed class SecondTrackingSeeder : IDatabaseSeeder
    {
        public int SeedCallCount { get; private set; }

        public Task SeedAsync(CancellationToken cancellationToken = default)
        {
            SeedCallCount++;
            return Task.CompletedTask;
        }
    }
}
