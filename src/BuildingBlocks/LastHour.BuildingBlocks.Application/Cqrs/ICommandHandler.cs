using LastHour.BuildingBlocks.SharedKernel.Results;
using MediatR;

namespace LastHour.BuildingBlocks.Application.Cqrs;

/// <summary>
/// Handles a command of type <typeparamref name="TCommand"/>. Inherits MediatR's
/// <see cref="IRequestHandler{TRequest, TResponse}"/> contract, so implementations provide
/// <c>Task&lt;Result&gt; Handle(TCommand command, CancellationToken cancellationToken)</c>
/// and are discovered automatically by MediatR's assembly scan rather than being registered
/// manually.
/// </summary>
/// <typeparam name="TCommand">The type of the command to handle.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}
