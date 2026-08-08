using Serilog;
using Serilog.Formatting.Compact;

namespace LastHour.Api.Observability.Auditing;

/// <summary>
/// Writes audit events to a dedicated Serilog file sink, independent of the operational logging
/// pipeline. Audit events are written as compact JSON so they can be consumed as structured data.
/// </summary>
public sealed class SerilogAuditLogger : IAuditLogger, IDisposable
{
    private readonly Serilog.Core.Logger _auditLog;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerilogAuditLogger"/> class.
    /// </summary>
    /// <param name="options">The audit logging options, including the audit file path.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public SerilogAuditLogger(AuditLoggingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _auditLog = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                path: options.FilePath,
                formatter: new CompactJsonFormatter(),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: options.RetainedFileCount)
            .CreateLogger();
    }

    /// <inheritdoc/>
    public void LogAudit(AuditEvent auditEvent)
    {
        if (auditEvent is null)
        {
            return;
        }

        _auditLog.Information(
            "Audit {EventType}: {Action} -> {Outcome}; StatusCode={StatusCode}; CorrelationId={CorrelationId}; " +
            "RemoteIpAddress={RemoteIpAddress}; Actor={Actor}",
            auditEvent.EventType,
            auditEvent.Action,
            auditEvent.Outcome,
            auditEvent.StatusCode,
            auditEvent.CorrelationId,
            auditEvent.RemoteIpAddress,
            auditEvent.Actor);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _auditLog.Dispose();
    }
}
