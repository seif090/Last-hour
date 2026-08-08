namespace LastHour.Api.Observability.Auditing;

/// <summary>
/// Options that configure the audit logging surface. Bound from the <c>AuditLogging</c>
/// configuration section.
/// </summary>
public sealed class AuditLoggingOptions
{
    /// <summary>
    /// The configuration section the options are bound from.
    /// </summary>
    public const string SectionName = "AuditLogging";

    /// <summary>
    /// Gets or sets a value indicating whether audit logging is enabled. When disabled the
    /// <see cref="IAuditLogger"/> writes nothing and the audit middleware is not registered.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the path of the dedicated audit log file. Audit events are written to their
    /// own file so security operations have an unfiltered trail independent of the operational
    /// logs. The rolling interval is one day.
    /// </summary>
    public string FilePath { get; set; } = "logs/last-hour-audit-.log";

    /// <summary>
    /// Gets or sets the number of rolling audit files to retain.
    /// </summary>
    public int RetainedFileCount { get; set; } = 30;
}
