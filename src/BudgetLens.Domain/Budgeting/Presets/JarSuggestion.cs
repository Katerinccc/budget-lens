using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Budgeting.Presets;

/// <summary>A suggested share of the income for one jar, identified by a stable, non-translated key.</summary>
public sealed record JarSuggestion(string Key, Percentage Share);
