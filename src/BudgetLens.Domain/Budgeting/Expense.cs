using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Budgeting;

/// <summary>
/// Money spent from a jar, optionally against one of its planned items (e.g. Rent). Recorded through
/// <see cref="MonthlyBudget.RecordExpense"/>, which checks the jar, the item and the date.
/// </summary>
public sealed record Expense
{
    public DateOnly Date { get; }

    public string JarName { get; }

    /// <summary>The planned item this expense pays, or null when it is just spending from the jar (e.g. a dinner from Fun).</summary>
    public string? ItemName { get; }

    public Money Amount { get; }

    /// <summary>Free text, e.g. "Dinner at restaurant". Null when not given.</summary>
    public string? Note { get; }

    internal Expense(DateOnly date, string jarName, string? itemName, Money amount, string? note)
    {
        Date = date;
        JarName = jarName;
        ItemName = itemName;
        Amount = amount;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }
}
