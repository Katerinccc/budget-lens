using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Budgeting;

/// <summary>A budget category (e.g. Fixed expenses, Fun). Its planned amount is the sum of its items.</summary>
public sealed class Jar
{
    public string Name { get; }

    public IReadOnlyList<PlannedItem> Items { get; }

    public Money Planned { get; }

    /// <summary>
    /// The preset suggestion this jar came from (e.g. "necessities"), or null for a custom jar.
    /// Links the jar to the suggestion even after the user renames it or switches language.
    /// </summary>
    public string? SuggestionKey { get; }

    public Jar(string name, IEnumerable<PlannedItem> items, string? suggestionKey = null)
    {
        Name = Names.Required(name, nameof(name));
        Items = [.. items];

        if (Items.Count == 0)
        {
            throw new ArgumentException("A jar needs at least one planned item.", nameof(items));
        }

        if (suggestionKey is not null && string.IsNullOrWhiteSpace(suggestionKey))
        {
            throw new ArgumentException("A suggestion key cannot be blank; use null for a custom jar.", nameof(suggestionKey));
        }

        Names.EnsureUnique(Items.Select(item => item.Name), "Item", nameof(items));
        Planned = Items.Skip(1).Aggregate(Items[0].Amount, (sum, item) => sum + item.Amount);
        SuggestionKey = suggestionKey;
    }

    /// <summary>A jar planned as one amount, e.g. Fun: 600.000.</summary>
    public static Jar Single(string name, Money amount, string? suggestionKey = null) =>
        new(name, [new PlannedItem(name, amount)], suggestionKey);
}
