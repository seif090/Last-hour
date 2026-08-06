using LastHour.BuildingBlocks.SharedKernel.Results;
using MediatR;

namespace LastHour.BuildingBlocks.Application.Cqrs;

/// <summary>
/// Handles a query of type <typeparamref name="TQuery"/> and produces a value of type
/// <typeparamref name="TResult"/>. Inherits MediatR's
/// <see cref="IRequestHandler{TRequest, TResponse}"/> contract, so implementations provide
/// <c>Task&lt;Result&lt;TResult&gt;&gt; Handle(TQuery query, CancellationToken cancellationToken)</c>
/// and are discovered automatically by MediatR's assembly scan rather than being registered
/// manually.
/// </summary>
/// <typeparam name="TQuery">The type of the query to handle.</typeparam>
/// <typeparam name="TResult">The type of the value produced by the query.</typeparam>
public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, Result<TResult>>
    where TQuery : IQuery<TResult>
{
}
