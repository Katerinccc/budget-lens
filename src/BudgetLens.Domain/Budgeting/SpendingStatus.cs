using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Budgeting;

/// <summary>Planned vs. spent for a jar or a planned item in one month.</summary>
public sealed record SpendingStatus(Money Planned, Money Spent)
{
    /// <summary>Planned − spent; negative when spending went over the plan.</summary>
    public Money Remaining => Planned - Spent;

    /// <summary>Spent as a percentage of planned, not rounded; above 100 when over the plan.</summary>
    public decimal UsedPercent => Spent.Amount / Planned.Amount * 100m;
}
