using Serilog;
using Serilog.Exceptions;

namespace LastHour.Api.Observability.Logging;

/// <summary>
/// Configures the Serilog pipeline for the LastHour API. Sinks (console, file, Seq) and their
/// verbosity are declared in the <c>Serilog</c> configuration section so operators can change
/// logging without a deployment. This extension adds the host-level enrichments that cannot be
/// expressed purely in configuration: the hosting environment name and the application version,
/// plus exception-detail rendering and the log context.
/// </summary>
public static class SerilogHostingExtensions
{
    /// <summary>
    /// Replaces the default Serilog setup with the LastHour configuration: configuration-driven
    /// sinks and enrichments, plus environment, application version and exception details.
    /// </summary>
    /// <param name="hostBuilder">The host builder to configure.</param>
    /// <returns>The same host builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="hostBuilder"/> is <see langword="null"/>.</exception>
    public static IHostBuilder UseLastHourSerilog(this IHostBuilder hostBuilder)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);

        hostBuilder.UseSerilog((context, services, configuration) =>
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithProperty("ApplicationVersion", ApplicationVersion.Get()));

        return hostBuilder;
    }
}
