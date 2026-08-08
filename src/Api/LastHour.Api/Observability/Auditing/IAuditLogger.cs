namespace LastHour.Api.Observability.Auditing;

/// <summary>
/// Records security-relevant events to the audit trail. The audit trail is separate from
/// operational logs and kept without the redaction or sample-rate concerns that apply there;
/// implementations must never write request bodies or other sensitive payloads.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Records an audit event.
    /// </summary>
    /// <param name="auditEvent">The event to record.</param>
    void LogAudit(AuditEvent auditEvent);
}
