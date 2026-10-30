using BudgetLens.Application.Abstractions;
using BudgetLens.Domain.Budgeting;
using BudgetLens.Domain.Common;

namespace BudgetLens.Application.Budgeting;

/// <summary>Creates the month's budget, or replaces its income and jars while keeping its expenses.</summary>
public sealed record SaveMonthlyBudgetCommand(
    int Year, int Month, decimal NetIncome, IReadOnlyList<JarInput> Jars, string Currency = Money.DefaultCurrency);

public sealed class SaveMonthlyBudgetHandler(IMonthlyBudgetRepository budgets)
    : ICommandHandler<SaveMonthlyBudgetCommand, MonthlySummaryDto>
{
    public async Task<MonthlySummaryDto> HandleAsync(SaveMonthlyBudgetCommand command, CancellationToken cancellationToken)
    {
        var month = new BudgetMonth(command.Year, command.Month);
        var netIncome = new Money(command.NetIncome, command.Currency);
        var jars = BudgetMapper.ToJars(command.Jars, command.Currency);

        var budget = await budgets.GetAsync(month, cancellationToken);
        if (budget is null)
        {
            budget = new MonthlyBudget(month, netIncome, jars);
        }
        else
        {
            budget.Replan(netIncome, jars);
        }

        await budgets.SaveAsync(budget, cancellationToken);
        return BudgetMapper.ToSummary(budget);
    }
}
