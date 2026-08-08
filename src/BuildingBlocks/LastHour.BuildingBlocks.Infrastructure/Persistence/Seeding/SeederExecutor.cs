using LastHour.BuildingBlocks.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LastHour.BuildingBlocks.Infrastructure.Persistence.Seeding;

/// <summary>
/// Executes registered <see cref="IDatabaseSeeder"/> implementations exactly once. Before a
/// seeder is invoked its type is looked up in the seeding history table; after the seeder
/// completes a history record is written, so subsequent initializer runs skip it.
/// </summary>
public sealed class SeederExecutor
{
    private static readonly Action<ILogger, string, Exception?> RunningSeeder =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1, nameof(RunningSeeder)),
            "Running database seeder {SeederType}.");

    private static readonly Action<ILogger, string, Exception?> SkippingSeeder =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(2, nameof(SkippingSeeder)),
            "Skipping database seeder {SeederType} because it has already run.");

    private readonly LastHourDbContext _dbContext;
    private readonly IClock _clock;
    private readonly ILogger<SeederExecutor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeederExecutor"/> class.
    /// </summary>
    /// <param name="dbContext">The database context used to query and update the seeding history.</param>
    /// <param name="clock">The clock used to timestamp history records.</param>
    /// <param name="logger">The logger used to record seeding diagnostics.</param>
    public SeederExecutor(LastHourDbContext dbContext, IClock clock, ILogger<SeederExecutor> logger)
    {
        _dbContext = dbContext;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    /// Runs the given seeders once each, skipping any whose type is already recorded in the
    /// seeding history.
    /// </summary>
    /// <param name="seeders">The seeders to execute.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that completes when the seeders have finished.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="seeders"/> is <see langword="null"/>.</exception>
    public async Task ExecuteAsync(IEnumerable<IDatabaseSeeder> seeders, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(seeders);

        foreach (IDatabaseSeeder seeder in seeders)
        {
            string seederType = seeder.GetType().Name;

            if (await AlreadyRanAsync(seederType, cancellationToken).ConfigureAwait(false))
            {
                SkippingSeeder(_logger, seederType, null);
                continue;
            }

            RunningSeeder(_logger, seederType, null);
            await seeder.SeedAsync(cancellationToken).ConfigureAwait(false);

            _dbContext.Add(SeedHistory.Create(seederType, _clock.UtcNow));
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private Task<bool> AlreadyRanAsync(string seederType, CancellationToken cancellationToken)
        => _dbContext.Set<SeedHistory>().AnyAsync(history => history.SeederType == seederType, cancellationToken);
}
