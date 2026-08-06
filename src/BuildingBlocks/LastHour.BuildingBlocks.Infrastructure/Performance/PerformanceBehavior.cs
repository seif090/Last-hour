using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.Performance;

/// <summary>
/// MediatR pipeline behavior that measures the execution time of every request and logs a warning
/// when it exceeds the configured slow-request threshold.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the request handler.</typeparam>
public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly Action<ILogger, string, double, TimeSpan, Exception?> SlowRequest =
        LoggerMessage.Define<string, double, TimeSpan>(
            LogLevel.Warning,
            new EventId(4, nameof(SlowRequest)),
            "Slow request {RequestName} took {ExecutionTimeMs} ms; threshold {SlowRequestThreshold}");

    private readonly ILogger _logger;
    private readonly TimeSpan _slowRequestThreshold;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record slow requests.</param>
    /// <param name="options">The options that provide the slow-request threshold.</param>
    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
        IOptions<PerformanceBehaviorOptions> options)
    {
        _logger = logger;
        _slowRequestThreshold = options.Value.SlowRequestThreshold;
    }

    /// <summary>
    /// Executes the pipeline, measuring the total execution time and logging a warning when it
    /// exceeds the configured threshold. The threshold is checked for successful and failed requests.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The delegate that invokes the next pipeline step.</param>
    /// <param name="cancellationToken">The token used to signal cancellation.</param>
    /// <returns>The handler result; the pipeline rethrows any exception after measuring it.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            stopwatch.Stop();
            double executionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
            if (executionTimeMs >= _slowRequestThreshold.TotalMilliseconds)
            {
                SlowRequest(_logger, requestName, executionTimeMs, _slowRequestThreshold, null);
            }
        }
    }
}
