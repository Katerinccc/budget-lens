namespace BudgetLens.Domain.Budgeting.Presets;

/// <summary>How a budget compares with a preset; jars that are not linked to any of its suggestions are listed apart.</summary>
public sealed record PresetComparison(IReadOnlyList<SuggestionComparison> Suggestions, IReadOnlyList<Jar> OtherJars);
