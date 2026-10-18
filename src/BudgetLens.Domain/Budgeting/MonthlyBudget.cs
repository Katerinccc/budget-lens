using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Budgeting;

/// <summary>
/// A month's net income and how the user plans to distribute it across jars. Users type amounts;
/// percentages are always derived from the income. Whatever is not planned is unassigned.
/// Expenses are recorded against its jars during the month.
/// </summary>
public sealed class MonthlyBudget
{
    private readonly List<Expense> _expenses = [];

    public BudgetMonth Month { get; }

    public Money NetIncome { get; }

    public IReadOnlyList<Jar> Jars { get; }

    public Money Planned { get; }

    /// <summary>Net income not yet planned in any jar. Never negative.</summary>
    public Money Unassigned { get; }

    public IReadOnlyList<Expense> Expenses => _expenses;

    public Money TotalSpent => SpentWhere(_ => true);

    public MonthlyBudget(BudgetMonth month, Money netIncome, IEnumerable<Jar> jars)
    {
        if (netIncome.Amount <= 0m)
        {
            throw new ArgumentException("The net income must be greater than zero.", nameof(netIncome));
        }

        Jars = [.. jars];
        Names.EnsureUnique(Jars.Select(jar => jar.Name), "Jar", nameof(jars));

        var planned = Jars.Aggregate(new Money(0m, netIncome.Currency), (sum, jar) => sum + jar.Planned);
        if (planned.Amount > netIncome.Amount)
        {
            throw new ArgumentException($"The planned amounts exceed the net income by {planned - netIncome}.", nameof(jars));
        }

        Month = month;
        NetIncome = netIncome;
        Planned = planned;
        Unassigned = netIncome - planned;
    }

    /// <summary>The percentage of the net income that <paramref name="amount"/> represents (a jar, an item or the unassigned amount).</summary>
    public Percentage ShareOfIncome(Money amount) => Percentage.Of(amount, NetIncome);

    /// <summary>
    /// Records money spent from an existing jar, optionally against one of its planned items. The date must be
    /// in this budget's month. Spending over the plan is allowed: it is reported, never blocked.
    /// </summary>
    public Expense RecordExpense(DateOnly date, string jarName, Money amount, string? itemName = null, string? note = null)
    {
        if (!Month.Contains(date))
        {
            throw new ArgumentException($"The date {date:yyyy-MM-dd} is not in {Month}.", nameof(date));
        }

        if (amount.Amount <= 0m)
        {
            throw new ArgumentException("An expense must be greater than zero.", nameof(amount));
        }

        Money.EnsureSameCurrency(amount, NetIncome);

        var jar = FindJar(jarName);
        var item = itemName is null ? null : FindItem(jar, itemName);

        var expense = new Expense(date, jar.Name, item?.Name, amount, note);
        _expenses.Add(expense);
        return expense;
    }

    /// <summary>Planned vs. spent for a jar, counting every expense recorded in it.</summary>
    public SpendingStatus StatusOf(string jarName)
    {
        var jar = FindJar(jarName);
        return new SpendingStatus(jar.Planned, SpentWhere(expense => expense.JarName == jar.Name));
    }

    /// <summary>Planned vs. spent for one planned item, counting only the expenses recorded against it.</summary>
    public SpendingStatus StatusOf(string jarName, string itemName)
    {
        var jar = FindJar(jarName);
        var item = FindItem(jar, itemName);
        return new SpendingStatus(item.Amount, SpentWhere(expense => expense.JarName == jar.Name && expense.ItemName == item.Name));
    }

    /// <summary>Starts a later month with the same income and jars (but no expenses), for the user to confirm or edit.</summary>
    public MonthlyBudget CopyTo(BudgetMonth month)
    {
        if (month.CompareTo(Month) <= 0)
        {
            throw new ArgumentException($"A budget can only be copied to a month after {Month}.", nameof(month));
        }

        return new MonthlyBudget(month, NetIncome, Jars);
    }

    private Jar FindJar(string jarName) =>
        Jars.FirstOrDefault(jar => Names.Comparer.Equals(jar.Name, jarName?.Trim()))
        ?? throw new ArgumentException($"There is no jar named '{jarName}' in {Month}.", nameof(jarName));

    private static PlannedItem FindItem(Jar jar, string itemName) =>
        jar.Items.FirstOrDefault(item => Names.Comparer.Equals(item.Name, itemName?.Trim()))
        ?? throw new ArgumentException($"The jar '{jar.Name}' has no planned item named '{itemName}'.", nameof(itemName));

    private Money SpentWhere(Func<Expense, bool> predicate) =>
        _expenses.Where(predicate).Aggregate(new Money(0m, NetIncome.Currency), (sum, expense) => sum + expense.Amount);
}
