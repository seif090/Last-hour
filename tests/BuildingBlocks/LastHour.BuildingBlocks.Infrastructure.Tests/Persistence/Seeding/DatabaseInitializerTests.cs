using LastHour.BuildingBlocks.Infrastructure.Persistence;
using LastHour.BuildingBlocks.Infrastructure.Persistence.Configuration;
using LastHour.BuildingBlocks.Infrastructure.Persistence.Seeding;
using LastHour.BuildingBlocks.Infrastructure.Tests.Persistence.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.Persistence.Seeding;

public class DatabaseInitializerTests
{
    [Fact]
    public async Task StartAsync_Disabled_DoesNothing()
    {
        var environment = new TestHostEnvironment { EnvironmentName = Environments.Development };
        using ServiceProvider provider = CreateProvider(out TrackingSeeder seeder);
        var initializer = new DatabaseInitializer(
            provider.GetRequiredService<IServiceScopeFactory>(),
            environment,
            Options.Create(new DatabaseInitializerOptions { Enabled = false }),
            provider.GetRequiredService<ILogger<DatabaseInitializer>>());

        await initializer.StartAsync(CancellationToken.None);

        Assert.Equal(0, seeder.SeedCallCount);
    }

    [Fact]
    public async Task StartAsync_OutsideDevelopment_DoesNothing()
    {
        var environment = new TestHostEnvironment { EnvironmentName = Environments.Production };
        using ServiceProvider provider = CreateProvider(out TrackingSeeder seeder);
        var initializer = new DatabaseInitializer(
            provider.GetRequiredService<IServiceScopeFactory>(),
            environment,
            Options.Create(new DatabaseInitializerOptions()),
            provider.GetRequiredService<ILogger<DatabaseInitializer>>());

        await initializer.StartAsync(CancellationToken.None);

        Assert.Equal(0, seeder.SeedCallCount);
    }

    [Fact]
    public async Task StartAsync_DevelopmentWithNonRelationalProvider_SkipsMigrationsAndSeeders()
    {
        var environment = new TestHostEnvironment { EnvironmentName = Environments.Development };
        using ServiceProvider provider = CreateProvider(out TrackingSeeder seeder);
        var initializer = new DatabaseInitializer(
            provider.GetRequiredService<IServiceScopeFactory>(),
            environment,
            Options.Create(new DatabaseInitializerOptions()),
            provider.GetRequiredService<ILogger<DatabaseInitializer>>());

        await initializer.StartAsync(CancellationToken.None);

        Assert.Equal(0, seeder.SeedCallCount);
    }

    private static ServiceProvider CreateProvider(out TrackingSeeder seeder)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<LastHourDbContext>(builder => builder.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddSingleton<IDatabaseSeeder, TrackingSeeder>();
        ServiceProvider provider = services.BuildServiceProvider();
        seeder = (TrackingSeeder)provider.GetRequiredService<IDatabaseSeeder>();
        return provider;
    }
}
