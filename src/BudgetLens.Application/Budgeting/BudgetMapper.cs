using BudgetLens.Domain.Budgeting;
using BudgetLens.Domain.Common;

namespace BudgetLens.Application.Budgeting;

/// <summary>Hand-written mapping between Domain types and the use-case DTOs (ADR 0004: no AutoMapper).</summary>
internal static class BudgetMapper
{
    public static IReadOnlyList<Jar> ToJars(IEnumerable<JarInput> jars, string currency) =>
    [
        .. jars.Select(jar => new Jar(
            jar.Name,
            jar.Items.Select(item => new PlannedItem(item.Name, new Money(item.Amount, currency))),
            jar.SuggestionKey)),
    ];

    public static JarDto ToDto(Jar jar, MonthlyBudget budget) => new(
        jar.Name,
        jar.SuggestionKey,
        jar.Planned.Amount,
        budget.ShareOfIncome(jar.Planned).Value,
        [.. jar.Items.Select(item => new PlannedItemDto(item.Name, item.Amount.Amount, budget.ShareOfIncome(item.Amount).Value))]);

    public static ExpenseDto ToDto(Expense expense) =>
        new(expense.Date, expense.JarName, expense.ItemName, expense.Amount.Amount, expense.Note);

    public static MonthlySummaryDto ToSummary(MonthlyBudget budget) => new(
        budget.Month.Year,
        budget.Month.Month,
        budget.NetIncome.Currency,
        budget.NetIncome.Amount,
        budget.Planned.Amount,
        budget.Unassigned.Amount,
        budget.ShareOfIncome(budget.Unassigned).Value,
        budget.TotalSpent.Amount,
        [
            .. budget.Jars.Select(jar => new JarSummaryDto(
                jar.Name,
                jar.SuggestionKey,
                budget.ShareOfIncome(jar.Planned).Value,
                ToDto(budget.StatusOf(jar.Name)),
                [
                    .. jar.Items.Select(item => new ItemSummaryDto(
                        item.Name,
                        budget.ShareOfIncome(item.Amount).Value,
                        ToDto(budget.StatusOf(jar.Name, item.Name)))),
                ])),
        ],
        [.. budget.Expenses.OrderBy(expense => expense.Date).Select(ToDto)]);

    private static SpendingDto ToDto(SpendingStatus status) =>
        new(status.Planned.Amount, status.Spent.Amount, status.Remaining.Amount, status.UsedPercent);
}
