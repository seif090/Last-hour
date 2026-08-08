namespace LastHour.Api.Middleware;

/// <summary>
/// Configures the correlation id contract of the API: the header name used on requests and
/// responses, whether the id is echoed on responses, and the maximum length accepted from
/// incoming headers before a new id is generated (a sanity bound against header abuse).
/// </summary>
public sealed class CorrelationIdOptions
{
    /// <summary>
    /// Gets the name of the configuration section the options bind from.
    /// </summary>
    public const string SectionName = "CorrelationId";

    /// <summary>
    /// Gets or sets the name of the correlation id header carried on requests and responses.
    /// </summary>
    public string HeaderName { get; set; } = CorrelationIdDefaults.HeaderName;

    /// <summary>
    /// Gets or sets a value indicating whether the correlation id is echoed on responses so
    /// callers can reference the request when reporting failures.
    /// </summary>
    public bool IncludeInResponse { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum length of an incoming correlation id. Longer values are rejected
    /// and replaced with a generated id to avoid unbounded header growth.
    /// </summary>
    public int MaximumIncomingLength { get; set; } = 100;
}
