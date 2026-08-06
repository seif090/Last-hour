using LastHour.BuildingBlocks.SharedKernel.Results;
using MediatR;

namespace LastHour.BuildingBlocks.Application.Cqrs;

/// <summary>
/// Marks a message that performs a read-only operation and produces a value of type
/// <typeparamref name="TResult"/>. A query is handled by an
/// <see cref="IQueryHandler{TQuery, TResult}"/> whose outcome is a
/// <see cref="Result{TResult}"/>. Derives from MediatR's
/// <see cref="IRequest{TResponse}"/> so queries can be dispatched through the MediatR
/// pipeline.
/// </summary>
/// <typeparam name="TResult">The type of the value produced by the query.</typeparam>
public interface IQuery<TResult> : IRequest<Result<TResult>>
{
}
