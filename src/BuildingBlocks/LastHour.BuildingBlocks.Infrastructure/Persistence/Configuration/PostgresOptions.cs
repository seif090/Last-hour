namespace LastHour.BuildingBlocks.Infrastructure.Persistence.Configuration;

/// <summary>
/// Configuration for the PostgreSQL data store. Bound from the <c>Postgres</c> configuration
/// section; the connection string additionally falls back to
/// <c>ConnectionStrings:Postgres</c> so containerized environments can override it with a
/// standard <c>ConnectionStrings__Postgres</c> environment variable.
/// </summary>
public sealed class PostgresOptions
{
    /// <summary>
    /// Gets the name of the configuration section this type binds to.
    /// </summary>
    public const string SectionName = "Postgres";

    /// <summary>
    /// Gets or sets the PostgreSQL connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether EF Core should include application data in
    /// generated logs and exceptions. Must remain disabled in production.
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether EF Core should include extended error details.
    /// Must remain disabled in production.
    /// </summary>
    public bool EnableDetailedErrors { get; set; }

    /// <summary>
    /// Gets or sets the command timeout in seconds, or 0 to leave the provider default.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; }

    /// <summary>
    /// Gets or sets the number of retry attempts for transient PostgreSQL failures.
    /// </summary>
    public int MaxRetryCount { get; set; } = 6;

    /// <summary>
    /// Gets or sets the maximum delay between retry attempts for transient failures.
    /// </summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the maximum number of pooled connections per database, or 0 for no limit.
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the minimum number of connections that are kept in the pool.
    /// </summary>
    public int MinPoolSize { get; set; }

    /// <summary>
    /// Gets or sets how long a pooled connection may remain idle before it is eligible to be
    /// closed when the pool is pruned.
    /// </summary>
    public TimeSpan ConnectionIdleLifetime { get; set; } = TimeSpan.FromSeconds(300);

    /// <summary>
    /// Gets or sets how often the pool scans for idle connections to prune.
    /// </summary>
    public TimeSpan ConnectionPruningInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets how long to wait while acquiring a connection from the pool before giving
    /// up, or <see cref="TimeSpan.Zero"/> to wait indefinitely.
    /// </summary>
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Gets or sets a value indicating whether the Npgsql data source enables dynamic JSON
    /// mapping so <c>jsonb</c> columns can be read as <see cref="System.Text.Json.JsonDocument"/>
    /// and written as objects without a dedicated POCO type.
    /// </summary>
    public bool EnableDynamicJson { get; set; } = true;
}
