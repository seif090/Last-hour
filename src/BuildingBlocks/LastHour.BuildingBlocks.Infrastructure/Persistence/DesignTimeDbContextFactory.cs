using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LastHour.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Creates <see cref="LastHourDbContext"/> instances for the EF Core design-time tooling
/// (<c>dotnet ef migrations</c>). This factory is never used at runtime; it exists solely so
/// migrations can be generated against PostgreSQL without a configured application host. The
/// connection string is a development placeholder that is only relevant while generating
/// migrations, which does not require a reachable database.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LastHourDbContext>
{
    /// <inheritdoc/>
    public LastHourDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<LastHourDbContext> options = new DbContextOptionsBuilder<LastHourDbContext>()
            .UseNpgsql("Host=localhost;Database=last_hour;Username=last_hour;Password=last_hour")
            .Options;

        return new LastHourDbContext(options);
    }
}
