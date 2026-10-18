using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Budgeting;

/// <summary>
/// A month's net income and how the user plans to distribute it across jars. Users type amounts;
/// percentages are always derived from the income. Whatever is not planned is unassigned.
/// </summary>
public sealed class MonthlyBudget
{
    public BudgetMonth Month { get; }

    public Money NetIncome { get; }

    public IReadOnlyList<Jar> Jars { get; }

    public Money Planned { get; }

    /// <summary>Net income not yet planned in any jar. Never negative.</summary>
    public Money Unassigned { get; }

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

    /// <summary>Starts a later month with the same income and jars, for the user to confirm or edit.</summary>
    public MonthlyBudget CopyTo(BudgetMonth month)
    {
        if (month.CompareTo(Month) <= 0)
        {
            throw new ArgumentException($"A budget can only be copied to a month after {Month}.", nameof(month));
        }

        return new MonthlyBudget(month, NetIncome, Jars);
    }
}
