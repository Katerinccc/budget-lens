namespace BudgetLens.Application.Abstractions;

/// <summary>Handles a query that only reads state and returns DTOs.</summary>
public interface IQueryHandler<in TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
