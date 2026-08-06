namespace LastHour.Api.Middleware;

/// <summary>
/// Centralizes the correlation id contract shared by the correlation middleware and the
/// exception handler: the request and response header name, the <see cref="HttpContext"/>
/// items key, and the Serilog log context property name.
/// </summary>
public static class CorrelationIdDefaults
{
    /// <summary>
    /// Gets the name of the correlation id header carried on requests and responses.
    /// </summary>
    public const string HeaderName = "X-Correlation-ID";

    /// <summary>
    /// Gets the key under which the correlation id is stored in <see cref="HttpContext.Items"/>
    /// and pushed into the Serilog log context.
    /// </summary>
    public const string ContextKey = "CorrelationId";
}
