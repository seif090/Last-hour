using System.Reflection;
using LastHour.BuildingBlocks.Infrastructure.Persistence.Configuration;
using LastHour.BuildingBlocks.Infrastructure.Persistence.Outbox;
using LastHour.BuildingBlocks.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;

namespace LastHour.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// The shared database context of the LastHour backend. It owns the outbox table and applies
/// cross-cutting concerns for every entity mapped into the model: automatic value conversion
/// for strongly typed identifiers, soft-delete query filters, and (via the registered
/// interceptors) audit stamps, soft deletion and outbox capture. Modules contribute entity
/// configurations by registering their assemblies with the persistence service collection
/// extension; nothing in the model is hard-coded here beyond the outbox.
/// </summary>
public class LastHourDbContext : DbContext
{
    private readonly IReadOnlyCollection<Assembly> _configurationAssemblies;

    /// <summary>
    /// Initializes a new instance of the <see cref="LastHourDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options, including the configured provider.</param>
    /// <param name="configurationAssemblies">Assemblies whose entity configurations are applied to the model.</param>
    public LastHourDbContext(DbContextOptions<LastHourDbContext> options, IEnumerable<Assembly>? configurationAssemblies = null)
        : base(options)
    {
        _configurationAssemblies = configurationAssemblies?.Distinct().ToArray() ?? Array.Empty<Assembly>();
    }

    /// <summary>
    /// Gets the outbox messages persisted by the domain events interceptor and the outbox publisher.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new SeedHistoryConfiguration());

        foreach (Assembly assembly in _configurationAssemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }

        modelBuilder.ApplyStronglyTypedIdConverters();
        modelBuilder.ApplySoftDeleteQueryFilters();
    }
}
