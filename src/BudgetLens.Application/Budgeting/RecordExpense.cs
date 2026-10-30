using BudgetLens.Application.Abstractions;
using BudgetLens.Domain.Budgeting;
using BudgetLens.Domain.Common;

namespace BudgetLens.Application.Budgeting;

/// <summary>Records money spent from a jar of the budget for the date's month. The amount is in that budget's currency.</summary>
public sealed record RecordExpenseCommand(DateOnly Date, string JarName, decimal Amount, string? ItemName = null, string? Note = null);

public sealed class RecordExpenseHandler(IMonthlyBudgetRepository budgets) : ICommandHandler<RecordExpenseCommand, ExpenseDto>
{
    public async Task<ExpenseDto> HandleAsync(RecordExpenseCommand command, CancellationToken cancellationToken)
    {
        var month = new BudgetMonth(command.Date.Year, command.Date.Month);
        var budget = await budgets.GetAsync(month, cancellationToken)
            ?? throw new NotFoundException($"There is no budget for {month}. Save the month's budget before recording expenses.");

        var expense = budget.RecordExpense(
            command.Date,
            command.JarName,
            new Money(command.Amount, budget.NetIncome.Currency),
            command.ItemName,
            command.Note);

        await budgets.SaveAsync(budget, cancellationToken);
        return BudgetMapper.ToDto(expense);
    }
}
