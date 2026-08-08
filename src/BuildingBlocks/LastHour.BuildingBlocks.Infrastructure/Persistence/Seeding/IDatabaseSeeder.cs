namespace LastHour.BuildingBlocks.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds reference or fixture data into the data store. Implementations are executed by the
/// <see cref="DatabaseInitializer"/> after migrations have been applied and must be idempotent:
/// running them more than once must not corrupt or duplicate data.
/// </summary>
public interface IDatabaseSeeder
{
    /// <summary>
    /// Seeds the data store with reference data.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that completes when seeding has finished.</returns>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
