namespace LastHour.Api.Observability.Telemetry;

/// <summary>
/// Configures OpenTelemetry for the API: service identity (name and version), which signals are
/// collected (tracing and metrics), and which exporters are enabled. Exporters stay off by
/// default so operators opt in per environment; the OTLP exporter is the standard integration
/// point for future backends (Grafana Tempo, Jaeger, Datadog, Elastic, ...) and only requires a
/// collector endpoint.
/// </summary>
public sealed class OpenTelemetryOptions
{
    /// <summary>
    /// Gets the name of the configuration section the options bind from.
    /// </summary>
    public const string SectionName = "OpenTelemetry";

    /// <summary>
    /// Gets or sets a value indicating whether OpenTelemetry is enabled at all.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether distributed tracing is collected.
    /// </summary>
    public bool TracingEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether metrics are collected.
    /// </summary>
    public bool MetricsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether traces and metrics are also written to the console
    /// (useful for local development and for validating the pipeline without a backend).
    /// </summary>
    public bool ConsoleExporterEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the OTLP exporter is used. When enabled,
    /// <see cref="OtlpEndpoint"/> must be configured.
    /// </summary>
    public bool UseOtlpExporter { get; set; }

    /// <summary>
    /// Gets or sets the OTLP (gRPC) collector endpoint, for example <c>http://collector:4317</c>.
    /// </summary>
    public string? OtlpEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the OpenTelemetry service name used for resource attributes.
    /// </summary>
    public string ServiceName { get; set; } = "LastHour.Api";

    /// <summary>
    /// Gets or sets the OpenTelemetry service version used for resource attributes.
    /// </summary>
    public string? ServiceVersion { get; set; }
}
