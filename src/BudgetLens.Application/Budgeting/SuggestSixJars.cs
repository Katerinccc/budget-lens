using BudgetLens.Application.Abstractions;
using BudgetLens.Domain.Budgeting.Presets;
using BudgetLens.Domain.Common;

namespace BudgetLens.Application.Budgeting;

/// <summary>Suggested Six Jars amounts for an income, to pre-fill the budget setup. Suggestions only.</summary>
public sealed record SuggestSixJarsQuery(decimal NetIncome, string Currency = Money.DefaultCurrency);

/// <param name="Key">Stable suggestion key (e.g. "necessities"); the UI translates it into a jar name.</param>
public sealed record SuggestedJarDto(string Key, decimal Share, decimal Amount);

public sealed class SuggestSixJarsHandler : IQueryHandler<SuggestSixJarsQuery, IReadOnlyList<SuggestedJarDto>>
{
    public Task<IReadOnlyList<SuggestedJarDto>> HandleAsync(SuggestSixJarsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<SuggestedJarDto> suggestions =
        [
            .. SixJarsPreset.SuggestAmounts(new Money(query.NetIncome, query.Currency))
                .Select(suggested => new SuggestedJarDto(suggested.Key, suggested.Share.Value, suggested.Amount.Amount)),
        ];

        return Task.FromResult(suggestions);
    }
}
