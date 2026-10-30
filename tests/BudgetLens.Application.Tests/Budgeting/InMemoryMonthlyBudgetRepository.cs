using BudgetLens.Application.Budgeting;
using BudgetLens.Domain.Budgeting;

namespace BudgetLens.Application.Tests.Budgeting;

/// <summary>Hand-written fake of the repository port: budgets kept in memory, saves counted.</summary>
internal sealed class InMemoryMonthlyBudgetRepository : IMonthlyBudgetRepository
{
    private readonly Dictionary<BudgetMonth, MonthlyBudget> _budgets = [];

    public int Saves { get; private set; }

    public Task<MonthlyBudget?> GetAsync(BudgetMonth month, CancellationToken cancellationToken) =>
        Task.FromResult(_budgets.GetValueOrDefault(month));

    public Task<MonthlyBudget?> GetLatestBeforeAsync(BudgetMonth month, CancellationToken cancellationToken) =>
        Task.FromResult(_budgets.Values
            .Where(budget => budget.Month.CompareTo(month) < 0)
            .OrderByDescending(budget => budget.Month)
            .FirstOrDefault());

    public Task SaveAsync(MonthlyBudget budget, CancellationToken cancellationToken)
    {
        _budgets[budget.Month] = budget;
        Saves++;
        return Task.CompletedTask;
    }
}
