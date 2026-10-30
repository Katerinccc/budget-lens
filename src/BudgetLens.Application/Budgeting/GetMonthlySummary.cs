using BudgetLens.Application.Abstractions;
using BudgetLens.Domain.Budgeting;

namespace BudgetLens.Application.Budgeting;

/// <summary>Planned vs. spent per jar and item for a month. The numbers come from the domain (ADR 0010).</summary>
public sealed record GetMonthlySummaryQuery(int Year, int Month);

public sealed class GetMonthlySummaryHandler(IMonthlyBudgetRepository budgets)
    : IQueryHandler<GetMonthlySummaryQuery, MonthlySummaryDto>
{
    public async Task<MonthlySummaryDto> HandleAsync(GetMonthlySummaryQuery query, CancellationToken cancellationToken)
    {
        var month = new BudgetMonth(query.Year, query.Month);
        var budget = await budgets.GetAsync(month, cancellationToken)
            ?? throw new NotFoundException($"There is no budget for {month}.");

        return BudgetMapper.ToSummary(budget);
    }
}
