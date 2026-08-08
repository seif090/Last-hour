using Microsoft.Extensions.Options;

namespace LastHour.Api.Observability.Auditing;

/// <summary>
/// Registers the audit logging surface: the <see cref="AuditLoggingOptions"/> are bound and
/// validated, the <see cref="IAuditLogger"/> is registered, and the audit middleware is added
/// to the pipeline.
/// </summary>
public static class AuditLoggingServiceCollectionExtensions
{
    /// <summary>
    /// Binds and validates the audit logging options and registers the <see cref="IAuditLogger"/>.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The application configuration used to bind settings.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/>
    /// is <see langword="null"/>.</exception>
    public static IServiceCollection AddLastHourAuditLogging(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<AuditLoggingOptions>()
            .Bind(configuration.GetSection(AuditLoggingOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<AuditLoggingOptions>, AuditLoggingOptionsValidator>();

        services.AddSingleton<IAuditLogger>(provider =>
        {
            AuditLoggingOptions options = provider.GetRequiredService<IOptions<AuditLoggingOptions>>().Value;
            return new SerilogAuditLogger(options);
        });

        services.AddScoped<AuditMiddleware>();

        return services;
    }

    /// <summary>
    /// Adds the audit middleware to the pipeline, after the request logger so the response status
    /// is final, and before routing so the endpoint and rate-limiter outcomes are observed.
    /// </summary>
    /// <param name="app">The application builder to extend.</param>
    /// <returns>The same application builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
    public static IApplicationBuilder UseLastHourAuditLogging(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<AuditMiddleware>();

        return app;
    }
}
