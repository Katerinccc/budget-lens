namespace BudgetLens.Application.Budgeting;

// DTOs exchanged with the driving adapters (gRPC, MCP), which must not see Domain types.
// Amounts are in the budget's currency; percentages are 0-100 and not rounded (the UI formats them).

public sealed record PlannedItemInput(string Name, decimal Amount);

/// <param name="SuggestionKey">The Six Jars suggestion this jar comes from (e.g. "necessities"), or null for a custom jar.</param>
public sealed record JarInput(string Name, IReadOnlyList<PlannedItemInput> Items, string? SuggestionKey = null);

public sealed record PlannedItemDto(string Name, decimal Amount, decimal ShareOfIncome);

public sealed record JarDto(string Name, string? SuggestionKey, decimal Planned, decimal ShareOfIncome, IReadOnlyList<PlannedItemDto> Items);

/// <param name="Remaining">Planned − spent; negative when over the plan.</param>
/// <param name="UsedPercent">Spent as a percentage of planned; above 100 when over the plan.</param>
public sealed record SpendingDto(decimal Planned, decimal Spent, decimal Remaining, decimal UsedPercent);

public sealed record ItemSummaryDto(string Name, decimal ShareOfIncome, SpendingDto Spending);

public sealed record JarSummaryDto(
    string Name, string? SuggestionKey, decimal ShareOfIncome, SpendingDto Spending, IReadOnlyList<ItemSummaryDto> Items);

public sealed record ExpenseDto(DateOnly Date, string JarName, string? ItemName, decimal Amount, string? Note);

public sealed record MonthlySummaryDto(
    int Year,
    int Month,
    string Currency,
    decimal NetIncome,
    decimal Planned,
    decimal Unassigned,
    decimal UnassignedShareOfIncome,
    decimal TotalSpent,
    IReadOnlyList<JarSummaryDto> Jars,
    IReadOnlyList<ExpenseDto> Expenses);
