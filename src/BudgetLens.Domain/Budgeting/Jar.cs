using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Budgeting;

/// <summary>A budget category (e.g. Fixed expenses, Fun). Its planned amount is the sum of its items.</summary>
public sealed class Jar
{
    public string Name { get; }

    public IReadOnlyList<PlannedItem> Items { get; }

    public Money Planned { get; }

    public Jar(string name, IEnumerable<PlannedItem> items)
    {
        Name = Names.Required(name, nameof(name));
        Items = [.. items];

        if (Items.Count == 0)
        {
            throw new ArgumentException("A jar needs at least one planned item.", nameof(items));
        }

        Names.EnsureUnique(Items.Select(item => item.Name), "Item", nameof(items));
        Planned = Items.Skip(1).Aggregate(Items[0].Amount, (sum, item) => sum + item.Amount);
    }

    /// <summary>A jar planned as one amount, e.g. Fun: 600.000.</summary>
    public static Jar Single(string name, Money amount) => new(name, [new PlannedItem(name, amount)]);
}
