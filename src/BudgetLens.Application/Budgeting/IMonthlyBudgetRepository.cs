using BudgetLens.Domain.Budgeting;

namespace BudgetLens.Application.Budgeting;

/// <summary>Port for loading and saving monthly budgets (with their expenses). Implemented in Infrastructure.</summary>
public interface IMonthlyBudgetRepository
{
    Task<MonthlyBudget?> GetAsync(BudgetMonth month, CancellationToken cancellationToken);

    /// <summary>The most recent budget before <paramref name="month"/>, used to pre-fill a new month.</summary>
    Task<MonthlyBudget?> GetLatestBeforeAsync(BudgetMonth month, CancellationToken cancellationToken);

    /// <summary>Adds the budget or saves its changes.</summary>
    Task SaveAsync(MonthlyBudget budget, CancellationToken cancellationToken);
}
