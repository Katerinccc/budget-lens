using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Budgeting.Presets;

/// <summary>
/// The user's share next to the suggested one. <see cref="DifferenceInPoints"/> is actual − suggested
/// (e.g. +10 means 10 percentage points above the suggestion).
/// </summary>
public sealed record SuggestionComparison(string Key, Percentage Suggested, Percentage Actual)
{
    public decimal DifferenceInPoints => Actual.Value - Suggested.Value;
}
