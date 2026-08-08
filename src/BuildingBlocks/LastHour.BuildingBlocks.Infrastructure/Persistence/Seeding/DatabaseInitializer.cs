using LastHour.BuildingBlocks.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.Persistence.Seeding;

/// <summary>
/// Applies pending EF Core migrations and runs registered <see cref="IDatabaseSeeder"/>
/// implementations when the host starts in the Development environment, unless disabled via
/// <see cref="DatabaseInitializerOptions.Enabled"/>. Seeders execute exactly once: the
/// <see cref="SeederExecutor"/> records completed seeders in the seeding history table so
/// repeated initializations do not duplicate data. The initializer is a no-op outside
/// Development, and it also skips non-relational providers (for example the in-memory provider
/// used by tests) because migrations are meaningless there.
/// </summary>
public sealed class DatabaseInitializer : IHostedService
{
    private static readonly Action<ILogger, Exception?> SkippedDisabled =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1, nameof(SkippedDisabled)),
            "Database initialization disabled; migrations and seeders will not run.");

    private static readonly Action<ILogger, Exception?> SkippedNonDevelopment =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(2, nameof(SkippedNonDevelopment)),
            "Database initialization skipped outside the Development environment.");

    private static readonly Action<ILogger, Exception?> SkippedNonRelational =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(3, nameof(SkippedNonRelational)),
            "Database initialization skipped for a non-relational provider.");

    private static readonly Action<ILogger, Exception?> ApplyingMigrations =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(4, nameof(ApplyingMigrations)),
            "Applying pending database migrations.");

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _environment;
    private readonly IOptions<DatabaseInitializerOptions> _options;
    private readonly ILogger<DatabaseInitializer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseInitializer"/> class.
    /// </summary>
    /// <param name="scopeFactory">The scope factory used to resolve a database context.</param>
    /// <param name="environment">The current host environment.</param>
    /// <param name="options">The database initializer options.</param>
    /// <param name="logger">The logger used to record initialization diagnostics.</param>
    public DatabaseInitializer(
        IServiceScopeFactory scopeFactory,
        IHostEnvironment environment,
        IOptions<DatabaseInitializerOptions> options,
        ILogger<DatabaseInitializer> logger)
    {
        _scopeFactory = scopeFactory;
        _environment = environment;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Value.Enabled)
        {
            SkippedDisabled(_logger, null);
            return;
        }

        if (!_environment.IsDevelopment())
        {
            SkippedNonDevelopment(_logger, null);
            return;
        }

        using IServiceScope scope = _scopeFactory.CreateScope();
        LastHourDbContext dbContext = scope.ServiceProvider.GetRequiredService<LastHourDbContext>();

        if (!dbContext.Database.IsRelational())
        {
            SkippedNonRelational(_logger, null);
            return;
        }

        ApplyingMigrations(_logger, null);
        await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

        SeederExecutor executor = scope.ServiceProvider.GetRequiredService<SeederExecutor>();
        await executor.ExecuteAsync(
                scope.ServiceProvider.GetServices<IDatabaseSeeder>(),
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
