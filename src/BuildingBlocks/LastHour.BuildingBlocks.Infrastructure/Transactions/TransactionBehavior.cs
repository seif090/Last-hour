using LastHour.BuildingBlocks.Application.Contracts;
using LastHour.BuildingBlocks.Application.Cqrs;
using LastHour.BuildingBlocks.SharedKernel.Results;
using MediatR;

namespace LastHour.BuildingBlocks.Infrastructure.Transactions;

/// <summary>
/// MediatR pipeline behavior that persists command changes atomically: it commits the unit of
/// work only when a command completes successfully and leaves it untouched on failure or
/// exception, so partial changes are never written. Queries pass through without touching the
/// unit of work.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The result type returned by the request handler.</typeparam>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work whose pending changes are committed after a successful command.</param>
    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Invokes the handler and commits pending changes only when the request is a command that
    /// completed successfully. Failed results and exceptions leave the unit of work uncommitted,
    /// and queries never reach the unit of work.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The delegate that invokes the next pipeline step.</param>
    /// <param name="cancellationToken">The token used to signal cancellation.</param>
    /// <returns>The handler result, committed to the data store when the request is a successful command.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICommand)
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        TResponse response = await next(cancellationToken).ConfigureAwait(false);

        if (response.IsSuccess)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return response;
    }
}
