namespace LastHour.BuildingBlocks.Infrastructure.Persistence.Configuration;

/// <summary>
/// Configuration for the database initializer. Bound from the <c>DatabaseInitializer</c>
/// configuration section.
/// </summary>
public sealed class DatabaseInitializerOptions
{
    /// <summary>
    /// Gets the name of the configuration section this type binds to.
    /// </summary>
    public const string SectionName = "DatabaseInitializer";

    /// <summary>
    /// Gets or sets a value indicating whether migrations are applied and seeders run on host
    /// startup in the Development environment. Disable this in integration-test hosts and in
    /// environments where migrations are applied out-of-band.
    /// </summary>
    public bool Enabled { get; set; } = true;
}
