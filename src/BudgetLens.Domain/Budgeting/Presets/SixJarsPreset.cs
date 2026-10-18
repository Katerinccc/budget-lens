using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Budgeting.Presets;

/// <summary>
/// The Six Jars method: a suggestion only. It proposes amounts for the user's income and compares the
/// user's own amounts with its percentages; it never changes or blocks what the user enters.
/// </summary>
public static class SixJarsPreset
{
    public const string Key = "six-jars";

    public static IReadOnlyList<JarSuggestion> Suggestions { get; } =
    [
        new("necessities", new Percentage(55m)),
        new("financial-freedom", new Percentage(10m)),
        new("long-term-savings", new Percentage(10m)),
        new("education", new Percentage(10m)),
        new("play", new Percentage(10m)),
        new("give", new Percentage(5m)),
    ];

    /// <summary>Suggested amounts in whole units that always add up to <paramref name="income"/>.</summary>
    public static IReadOnlyList<SuggestedAmount> SuggestAmounts(Money income)
    {
        var amounts = income.AllocateWholeUnits([.. Suggestions.Select(suggestion => suggestion.Share)]);

        return [.. Suggestions.Zip(amounts, (suggestion, amount) => new SuggestedAmount(suggestion.Key, suggestion.Share, amount))];
    }

    /// <summary>
    /// The budget's share of income per suggestion (jars linked by <see cref="Jar.SuggestionKey"/>; 0% when
    /// none is linked), plus the jars that do not belong to this preset.
    /// </summary>
    public static PresetComparison Compare(MonthlyBudget budget)
    {
        var suggestions = Suggestions
            .Select(suggestion =>
            {
                var actual = budget.Jars
                    .Where(jar => jar.SuggestionKey == suggestion.Key)
                    .Aggregate(new Money(0m, budget.NetIncome.Currency), (sum, jar) => sum + jar.Planned);

                return new SuggestionComparison(suggestion.Key, suggestion.Share, budget.ShareOfIncome(actual));
            })
            .ToList();

        var otherJars = budget.Jars
            .Where(jar => !Suggestions.Any(suggestion => suggestion.Key == jar.SuggestionKey))
            .ToList();

        return new PresetComparison(suggestions, otherJars);
    }
}
