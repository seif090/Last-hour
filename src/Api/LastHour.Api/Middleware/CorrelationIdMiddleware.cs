using Serilog.Context;

namespace LastHour.Api.Middleware;

/// <summary>
/// Assigns a correlation id to every request and makes it available across the pipeline.
/// An incoming <see cref="CorrelationIdDefaults.HeaderName"/> header is honored when present;
/// otherwise a new id is generated. The id is stored on the <see cref="HttpContext"/>, pushed
/// into the Serilog log context for the duration of the request, and echoed on the response so
/// callers can reference the request when reporting failures.
/// </summary>
public sealed class CorrelationIdMiddleware : IMiddleware
{
    private const int MaximumIncomingLength = 100;

    /// <summary>
    /// Stores the correlation id and invokes the remainder of the pipeline.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <returns>A task representing the remainder of the request pipeline.</returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        string correlationId = GetOrCreateCorrelationId(context.Request);

        context.Items[CorrelationIdDefaults.ContextKey] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdDefaults.HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty(CorrelationIdDefaults.ContextKey, correlationId))
        {
            await next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpRequest request)
    {
        string? incoming = request.Headers[CorrelationIdDefaults.HeaderName];
        if (!string.IsNullOrWhiteSpace(incoming) && incoming.Length <= MaximumIncomingLength)
        {
            return incoming;
        }

        return Guid.NewGuid().ToString("N");
    }
}
