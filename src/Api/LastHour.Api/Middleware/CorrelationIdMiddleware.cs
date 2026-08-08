using System.Diagnostics;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace LastHour.Api.Middleware;

/// <summary>
/// Assigns a correlation id to every request and makes it available across the pipeline.
/// An incoming <see cref="CorrelationIdOptions.HeaderName"/> header is honored when present;
/// otherwise the ambient OpenTelemetry trace id is reused when available and a new id is
/// generated as a last resort. The id is stored on the <see cref="HttpContext"/>, pushed into
/// the Serilog log context for the duration of the request, published as an OpenTelemetry
/// <c>correlation.id</c> tag, and echoed on the response so callers can reference the request
/// when reporting failures.
/// </summary>
public sealed class CorrelationIdMiddleware : IMiddleware
{
    private readonly IOptions<CorrelationIdOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationIdMiddleware"/> class.
    /// </summary>
    /// <param name="options">The correlation id options.</param>
    public CorrelationIdMiddleware(IOptions<CorrelationIdOptions> options)
    {
        _options = options;
    }

    /// <summary>
    /// Stores the correlation id and invokes the remainder of the pipeline.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <returns>A task representing the remainder of the request pipeline.</returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        CorrelationIdOptions options = _options.Value;
        string correlationId = GetOrCreateCorrelationId(context.Request, options);

        context.Items[CorrelationIdDefaults.ContextKey] = correlationId;

        Activity.Current?.SetTag(CorrelationIdDefaults.ContextKey, correlationId);

        if (options.IncludeInResponse)
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[options.HeaderName] = correlationId;
                return Task.CompletedTask;
            });
        }

        using (LogContext.PushProperty(CorrelationIdDefaults.ContextKey, correlationId))
        {
            await next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpRequest request, CorrelationIdOptions options)
    {
        string? incoming = request.Headers[options.HeaderName];
        if (!string.IsNullOrWhiteSpace(incoming) && incoming.Length <= options.MaximumIncomingLength)
        {
            return incoming;
        }

        return GenerateCorrelationId();
    }

    private static string GenerateCorrelationId()
    {
        Activity? activity = Activity.Current;
        if (activity is not null && activity.TraceId != default)
        {
            return activity.TraceId.ToString();
        }

        return Guid.NewGuid().ToString("N");
    }
}
