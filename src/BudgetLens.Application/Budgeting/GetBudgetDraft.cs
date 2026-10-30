using BudgetLens.Application.Abstractions;
using BudgetLens.Domain.Budgeting;
using BudgetLens.Domain.Common;

namespace BudgetLens.Application.Budgeting;

/// <summary>The budget a month starts from when the user opens its setup screen.</summary>
public sealed record GetBudgetDraftQuery(int Year, int Month);

public enum BudgetDraftSource
{
    /// <summary>The month already has a saved budget.</summary>
    Saved,

    /// <summary>Not saved yet: income and jars copied from the latest earlier month, for the user to confirm or edit.</summary>
    CopiedFromEarlierMonth,

    /// <summary>No budget in this or any earlier month: the user starts from scratch (or from Six Jars).</summary>
    Empty,
}

/// <param name="CopiedFrom">The month the draft was copied from ("yyyy-MM"), when <see cref="Source"/> is CopiedFromEarlierMonth.</param>
/// <param name="NetIncome">Null when the draft is empty.</param>
public sealed record BudgetDraftDto(
    int Year,
    int Month,
    BudgetDraftSource Source,
    string? CopiedFrom,
    string Currency,
    decimal? NetIncome,
    decimal Planned,
    decimal? Unassigned,
    IReadOnlyList<JarDto> Jars);

public sealed class GetBudgetDraftHandler(IMonthlyBudgetRepository budgets) : IQueryHandler<GetBudgetDraftQuery, BudgetDraftDto>
{
    public async Task<BudgetDraftDto> HandleAsync(GetBudgetDraftQuery query, CancellationToken cancellationToken)
    {
        var month = new BudgetMonth(query.Year, query.Month);

        var saved = await budgets.GetAsync(month, cancellationToken);
        if (saved is not null)
        {
            return ToDraft(saved, BudgetDraftSource.Saved, copiedFrom: null);
        }

        var earlier = await budgets.GetLatestBeforeAsync(month, cancellationToken);
        if (earlier is not null)
        {
            return ToDraft(earlier.CopyTo(month), BudgetDraftSource.CopiedFromEarlierMonth, earlier.Month.ToString());
        }

        return new BudgetDraftDto(
            month.Year, month.Month, BudgetDraftSource.Empty, null, Money.DefaultCurrency, null, 0m, null, []);
    }

    private static BudgetDraftDto ToDraft(MonthlyBudget budget, BudgetDraftSource source, string? copiedFrom) => new(
        budget.Month.Year,
        budget.Month.Month,
        source,
        copiedFrom,
        budget.NetIncome.Currency,
        budget.NetIncome.Amount,
        budget.Planned.Amount,
        budget.Unassigned.Amount,
        [.. budget.Jars.Select(jar => BudgetMapper.ToDto(jar, budget))]);
}
