using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Budgeting.Presets;

/// <summary>A suggestion turned into an amount for a given income.</summary>
public sealed record SuggestedAmount(string Key, Percentage Share, Money Amount);
