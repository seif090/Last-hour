namespace LastHour.BuildingBlocks.Infrastructure.Persistence.Seeding;

/// <summary>
/// Records that a database seeder has already completed so that seeders run exactly once.
/// The <see cref="SeederType"/> value is unique: the database initializer consults this table
/// before invoking a seeder and inserts a row once the seeder succeeds, and the unique index
/// guards against duplicate records from concurrent initializers.
/// </summary>
public sealed class SeedHistory
{
    private SeedHistory(Guid id, string seederType, DateTime executedAtUtc)
    {
        Id = id;
        SeederType = seederType;
        ExecutedAtUtc = executedAtUtc;
    }

    /// <summary>
    /// Gets the record identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the short name of the seeder type that completed.
    /// </summary>
    public string SeederType { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp at which the seeder completed.
    /// </summary>
    public DateTime ExecutedAtUtc { get; private set; }

    /// <summary>
    /// Creates a history record for a completed seeder.
    /// </summary>
    /// <param name="seederType">The short name of the seeder type.</param>
    /// <param name="executedAtUtc">The UTC timestamp at which the seeder completed.</param>
    /// <returns>A new history record.</returns>
    public static SeedHistory Create(string seederType, DateTime executedAtUtc)
        => new SeedHistory(Guid.NewGuid(), seederType, executedAtUtc);
}
