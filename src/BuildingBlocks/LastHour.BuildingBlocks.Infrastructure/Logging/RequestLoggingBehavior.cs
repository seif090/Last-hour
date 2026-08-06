using System.Diagnostics;
using LastHour.BuildingBlocks.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LastHour.BuildingBlocks.Infrastructure.Logging;

/// <summary>
/// MediatR pipeline behavior that records structured diagnostics for every request:
/// the request name, execution time, correlation id and outcome. Failures are logged
/// with their machine-readable error type and codes, never with payload data.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The result type returned by the request handler.</typeparam>
public sealed class RequestLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private static readonly Action<ILogger, string, double, string?, Exception?> CompletedRequest =
        LoggerMessage.Define<string, double, string?>(
            LogLevel.Information,
            new EventId(1, nameof(CompletedRequest)),
            "Completed request {RequestName} in {ExecutionTimeMs} ms; CorrelationId={CorrelationId}");

    private static readonly Action<ILogger, string, double, string?, Exception?> FailedRequest =
        LoggerMessage.Define<string, double, string?>(
            LogLevel.Error,
            new EventId(2, nameof(FailedRequest)),
            "Failed request {RequestName} after {ExecutionTimeMs} ms; CorrelationId={CorrelationId}");

    private static readonly Action<ILogger, string, double, string?, ErrorType, string[], Exception?> FailedResult =
        LoggerMessage.Define<string, double, string?, ErrorType, string[]>(
            LogLevel.Warning,
            new EventId(3, nameof(FailedResult)),
            "Failed request {RequestName} in {ExecutionTimeMs} ms; CorrelationId={CorrelationId}; ErrorType={ErrorType}; ErrorCodes={ErrorCodes}");

    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLoggingBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record request diagnostics.</param>
    public RequestLoggingBehavior(ILogger<RequestLoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Executes the pipeline, logging the request name, execution time, correlation id and outcome.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The delegate that invokes the next pipeline step.</param>
    /// <param name="cancellationToken">The token used to signal cancellation.</param>
    /// <returns>The handler result; the pipeline rethrows any exception after logging it.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        string? correlationId = GetCorrelationId();
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            TResponse response = await next(cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();
            LogCompleted(requestName, stopwatch.Elapsed.TotalMilliseconds, correlationId, response);

            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            FailedRequest(_logger, requestName, stopwatch.Elapsed.TotalMilliseconds, correlationId, exception);

            throw;
        }
    }

    /// <summary>
    /// Gets the correlation id of the current operation, when one is available.
    /// </summary>
    /// <returns>The trace id of the current activity, or <see langword="null"/> when there is none.</returns>
    private static string? GetCorrelationId()
    {
        Activity? activity = Activity.Current;
        return activity is null || activity.TraceId == default ? null : activity.TraceId.ToString();
    }

    /// <summary>
    /// Gets the error codes of a failed result; descriptions are deliberately excluded because they may carry sensitive data.
    /// </summary>
    /// <param name="response">The failed result.</param>
    /// <returns>The machine-readable error codes.</returns>
    private static string[] GetErrorCodes(TResponse response)
        => response.Errors.Select(error => error.Code).ToArray();

    /// <summary>
    /// Records the outcome of a completed request.
    /// </summary>
    /// <param name="requestName">The name of the request.</param>
    /// <param name="executionTimeMs">The execution time in milliseconds.</param>
    /// <param name="correlationId">The correlation id, when available.</param>
    /// <param name="response">The result produced by the handler.</param>
    private void LogCompleted(string requestName, double executionTimeMs, string? correlationId, TResponse response)
    {
        if (response.IsSuccess)
        {
            CompletedRequest(_logger, requestName, executionTimeMs, correlationId, null);
        }
        else
        {
            FailedResult(_logger, requestName, executionTimeMs, correlationId, response.ErrorType, GetErrorCodes(response), null);
        }
    }
}
