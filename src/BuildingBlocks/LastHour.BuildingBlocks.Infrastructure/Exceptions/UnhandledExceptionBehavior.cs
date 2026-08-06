using System.Runtime.InteropServices;
using LastHour.BuildingBlocks.Infrastructure.Results;
using LastHour.BuildingBlocks.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LastHour.BuildingBlocks.Infrastructure.Exceptions;

/// <summary>
/// MediatR pipeline behavior that converts unexpected exceptions raised while handling a request
/// into failed results. The exception is logged with its full stack trace before any conversion,
/// and critical system failures and cancellations are always rethrown so they are never swallowed.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The result type returned by the request handler.</typeparam>
public sealed class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private static readonly Action<ILogger, string, Exception?> UnhandledException =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(5, nameof(UnhandledException)),
            "Unhandled exception while processing request {RequestName}");

    private static readonly Func<Error, TResponse> FailedResultFactory = ResultFailureFactory.Create<TResponse>();

    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnhandledExceptionBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record unhandled exceptions.</param>
    public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Invokes the next pipeline step, logging any exception that reaches this behavior. Non-critical
    /// exceptions are converted into a failed result; critical system failures and cancellations are
    /// rethrown with their original stack trace.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The delegate that invokes the next pipeline step.</param>
    /// <param name="cancellationToken">The token used to signal cancellation.</param>
    /// <returns>The handler result, or a failed result when an unexpected exception occurred.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            UnhandledException(_logger, typeof(TRequest).Name, exception);

            if (ShouldRethrow(exception))
            {
                throw;
            }

            return FailedResultFactory(Error.Failure("UnhandledException", exception.Message));
        }
    }

    /// <summary>
    /// Determines whether an exception must propagate instead of being converted into a result.
    /// Critical system failures and request cancellations are never converted.
    /// </summary>
    /// <param name="exception">The exception raised while handling the request.</param>
    /// <returns><see langword="true"/> when the exception must propagate; otherwise <see langword="false"/>.</returns>
    private static bool ShouldRethrow(Exception exception)
        => exception is OperationCanceledException
            or AccessViolationException
            or AppDomainUnloadedException
            or BadImageFormatException
            or CannotUnloadAppDomainException
            or InvalidProgramException
            or OutOfMemoryException
            or SEHException
            or StackOverflowException
            or ThreadAbortException;
}
