using LastHour.BuildingBlocks.Infrastructure.Persistence.Interceptors;
using LastHour.BuildingBlocks.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using Npgsql;

namespace LastHour.BuildingBlocks.Infrastructure.Persistence.Configuration;

/// <summary>
/// Configures the <see cref="LastHourDbContext"/> options from <see cref="PostgresOptions"/> and the
/// shared <see cref="NpgsqlDataSource"/>: provider, transient-failure retry policy, command timeout,
/// diagnostic flags and the persistence interceptors.
/// </summary>
public sealed class LastHourDbContextOptionsSetup : IDbContextOptionsConfiguration<LastHourDbContext>
{
    private readonly IOptions<PostgresOptions> _postgresOptions;
    private readonly NpgsqlDataSource _dataSource;
    private readonly AuditInterceptor _auditInterceptor;
    private readonly SoftDeleteInterceptor _softDeleteInterceptor;
    private readonly DomainEventsInterceptor _domainEventsInterceptor;

    /// <summary>
    /// Initializes a new instance of the <see cref="LastHourDbContextOptionsSetup"/> class.
    /// </summary>
    /// <param name="postgresOptions">The PostgreSQL options.</param>
    /// <param name="dataSource">The shared Npgsql data source.</param>
    /// <param name="auditInterceptor">The audit stamping interceptor.</param>
    /// <param name="softDeleteInterceptor">The soft delete interceptor.</param>
    /// <param name="domainEventsInterceptor">The domain events outbox interceptor.</param>
    public LastHourDbContextOptionsSetup(
        IOptions<PostgresOptions> postgresOptions,
        NpgsqlDataSource dataSource,
        AuditInterceptor auditInterceptor,
        SoftDeleteInterceptor softDeleteInterceptor,
        DomainEventsInterceptor domainEventsInterceptor)
    {
        _postgresOptions = postgresOptions;
        _dataSource = dataSource;
        _auditInterceptor = auditInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
        _domainEventsInterceptor = domainEventsInterceptor;
    }

    /// <inheritdoc/>
    public void Configure(IServiceProvider serviceProvider, DbContextOptionsBuilder optionsBuilder)
        => ConfigureCore((DbContextOptionsBuilder<LastHourDbContext>)optionsBuilder);

    private void ConfigureCore(DbContextOptionsBuilder<LastHourDbContext> options)
    {
        PostgresOptions postgres = _postgresOptions.Value;

        options.UseNpgsql(
            _dataSource,
            npgsql =>
            {
                npgsql.EnableRetryOnFailure(
                    postgres.MaxRetryCount,
                    postgres.MaxRetryDelay,
                    errorCodesToAdd: null);

                if (postgres.CommandTimeoutSeconds > 0)
                {
                    npgsql.CommandTimeout(postgres.CommandTimeoutSeconds);
                }
            });

        if (postgres.EnableDetailedErrors)
        {
            options.EnableDetailedErrors();
        }

        if (postgres.EnableSensitiveDataLogging)
        {
            options.EnableSensitiveDataLogging();
        }

        options.AddInterceptors(
            _auditInterceptor,
            _softDeleteInterceptor,
            _domainEventsInterceptor);
    }
}
