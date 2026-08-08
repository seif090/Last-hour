using LastHour.BuildingBlocks.Application.Contracts;
using LastHour.BuildingBlocks.Infrastructure.Persistence.Seeding;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.Persistence.TestSupport;

public sealed class FixedClock : IClock
{
    public DateTime UtcNow { get; set; } = TestPersistence.FixedUtcNow;
}

public sealed class FixedCurrentUser : ICurrentUser
{
    public bool IsAuthenticated => UserId is not null;

    public string? UserId { get; set; } = TestPersistence.UserId;

    public string? Name => UserId;

    public IReadOnlyCollection<string> Roles => Array.Empty<string>();

    public bool IsInRole(string role) => false;
}

public sealed class TestHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = Environments.Development;

    public string ApplicationName { get; set; } = "Test";

    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}

public sealed class TrackingSeeder : IDatabaseSeeder
{
    public int SeedCallCount { get; private set; }

    public Task SeedAsync(CancellationToken cancellationToken = default)
    {
        SeedCallCount++;
        return Task.CompletedTask;
    }
}
