using System.Diagnostics;
using LastHour.Api.Middleware;
using LastHour.BuildingBlocks.Application.Contracts;
using Serilog.Context;

namespace LastHour.Api.Observability.RequestLogging;

/// <summary>
/// Records one structured log event per HTTP request: method, path, status code, execution time,
/// remote IP address, user agent, correlation id, request id and the authenticated user, when one
/// is present. Request bodies, query strings, headers, passwords, tokens and secrets are never
/// logged. The request id is also pushed into the Serilog log context so the current request is
/// identifiable in every log statement emitted while it is being processed.
/// </summary>
public sealed class RequestLoggingMiddleware : IMiddleware
{
    private static readonly Action<ILogger, string, string, int, double, string, Exception?> RequestCompleted =
        LoggerMessage.Define<string, string, int, double, string>(
            LogLevel.Information,
            new EventId(10, nameof(RequestCompleted)),
            "HTTP {Method} {Path} responded {StatusCode} in {ExecutionTimeMs} ms; CorrelationId={CorrelationId}");

    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record request diagnostics.</param>
    /// <param name="currentUser">The ambient identity of the current request.</param>
    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger, ICurrentUser currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Executes the remainder of the pipeline and logs the request outcome.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <returns>A task representing the remainder of the request pipeline.</returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        string correlationId = GetCorrelationId(context);
        string method = context.Request.Method;
        string path = context.Request.Path.Value ?? "/";
        string requestId = context.TraceIdentifier;

        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            using (LogContext.PushProperty("RequestId", requestId))
            {
                await next(context);
            }
        }
        finally
        {
            stopwatch.Stop();

            using (LogContext.PushProperty("RemoteIpAddress", GetRemoteIpAddress(context)))
            using (LogContext.PushProperty("UserAgent", GetUserAgent(context)))
            using (LogContext.PushProperty("UserId", _currentUser.UserId ?? "anonymous"))
            using (LogContext.PushProperty("Authenticated", _currentUser.IsAuthenticated))
            {
                RequestCompleted(
                    _logger,
                    method,
                    path,
                    context.Response.StatusCode,
                    stopwatch.Elapsed.TotalMilliseconds,
                    correlationId,
                    null);
            }
        }
    }

    private static string GetCorrelationId(HttpContext context)
        => context.Items.TryGetValue(CorrelationIdDefaults.ContextKey, out object? value) && value is string id
            ? id
            : string.Empty;

    private static string GetRemoteIpAddress(HttpContext context)
        => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static string GetUserAgent(HttpContext context)
    {
        string? userAgent = context.Request.Headers.UserAgent.ToString();
        return string.IsNullOrWhiteSpace(userAgent) ? "unknown" : userAgent;
    }
}
