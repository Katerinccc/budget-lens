namespace BudgetLens.Application.Abstractions;

/// <summary>Handles a command that changes state (see ADR 0004: plain handlers resolved through DI, no MediatR).</summary>
public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
