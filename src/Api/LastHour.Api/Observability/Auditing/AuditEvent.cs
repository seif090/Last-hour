namespace LastHour.Api.Observability.Auditing;

/// <summary>
/// A security-relevant event recorded to the audit trail. Audit events are deliberately sparse:
/// they describe <em>what</em> happened (the action and outcome) with enough context to trace the
/// request, and never carry request bodies, query strings or other potentially sensitive payloads.
/// </summary>
public sealed record AuditEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditEvent"/> class.
    /// </summary>
    /// <param name="eventType">A stable machine-readable event type, for example
    /// <c>AccessRejected</c> or <c>ServerError</c>.</param>
    /// <param name="action">The action that was attempted, for example <c>GET /health/ready</c>.</param>
    /// <param name="outcome">The outcome of the action, for example <c>Denied</c> or <c>Failed</c>.</param>
    /// <param name="statusCode">The HTTP status code of the response, when applicable.</param>
    /// <param name="correlationId">The correlation id of the request, when available.</param>
    /// <param name="remoteIpAddress">The client address, when available.</param>
    /// <param name="actor">The acting principal, when authentication is available.</param>
    public AuditEvent(
        string eventType,
        string action,
        string outcome,
        int? statusCode = null,
        string? correlationId = null,
        string? remoteIpAddress = null,
        string? actor = null)
    {
        EventType = eventType;
        Action = action;
        Outcome = outcome;
        StatusCode = statusCode;
        CorrelationId = correlationId;
        RemoteIpAddress = remoteIpAddress;
        Actor = actor;
    }

    /// <summary>
    /// Gets the point in time (UTC) the event occurred.
    /// </summary>
    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the stable, machine-readable event type.
    /// </summary>
    public string EventType { get; }

    /// <summary>
    /// Gets the action that was attempted.
    /// </summary>
    public string Action { get; }

    /// <summary>
    /// Gets the outcome of the action.
    /// </summary>
    public string Outcome { get; }

    /// <summary>
    /// Gets the HTTP status code of the response, when applicable.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Gets the correlation id of the request, when available.
    /// </summary>
    public string? CorrelationId { get; }

    /// <summary>
    /// Gets the client address, when available.
    /// </summary>
    public string? RemoteIpAddress { get; }

    /// <summary>
    /// Gets the acting principal, when authentication is available.
    /// </summary>
    public string? Actor { get; }
}
