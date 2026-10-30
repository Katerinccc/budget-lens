using BudgetLens.Application.Abstractions;
using BudgetLens.Application.Budgeting;

namespace BudgetLens.Application.Tests.Budgeting;

public class BudgetUseCaseTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    private readonly InMemoryMonthlyBudgetRepository _budgets = new();

    private static SaveMonthlyBudgetCommand OctoberPlan(decimal netIncome = 10_000_000m, decimal fun = 600_000m) => new(
        2026, 10, netIncome,
        [
            new JarInput("Fixed expenses",
            [
                new PlannedItemInput("Rent", 2_800_000m),
                new PlannedItemInput("Utilities", 450_000m),
            ], "necessities"),
            new JarInput("Fun", [new PlannedItemInput("Fun", fun)], "play"),
            new JarInput("Savings", [new PlannedItemInput("Savings", 3_000_000m)], "financial-freedom"),
        ]);

    private Task<MonthlySummaryDto> Save(SaveMonthlyBudgetCommand command) =>
        new SaveMonthlyBudgetHandler(_budgets).HandleAsync(command, None);

    private Task<ExpenseDto> Record(RecordExpenseCommand command) =>
        new RecordExpenseHandler(_budgets).HandleAsync(command, None);

    private Task<MonthlySummaryDto> Summary(int year, int month) =>
        new GetMonthlySummaryHandler(_budgets).HandleAsync(new GetMonthlySummaryQuery(year, month), None);

    private Task<BudgetDraftDto> Draft(int year, int month) =>
        new GetBudgetDraftHandler(_budgets).HandleAsync(new GetBudgetDraftQuery(year, month), None);

    [Fact]
    public async Task Saving_a_new_month_creates_its_budget_and_returns_its_summary()
    {
        var summary = await Save(OctoberPlan());

        Assert.Equal("COP", summary.Currency);
        Assert.Equal(10_000_000m, summary.NetIncome);
        Assert.Equal(6_850_000m, summary.Planned);
        Assert.Equal(3_150_000m, summary.Unassigned);
        Assert.Equal(31.5m, summary.UnassignedShareOfIncome);
        Assert.Equal(["Fixed expenses", "Fun", "Savings"], summary.Jars.Select(jar => jar.Name));
        Assert.Equal(30m, summary.Jars[2].ShareOfIncome);
        Assert.Equal("necessities", summary.Jars[0].SuggestionKey);
        Assert.Equal(1, _budgets.Saves);
    }

    [Fact]
    public async Task Saving_an_existing_month_replans_it_and_keeps_its_expenses()
    {
        await Save(OctoberPlan());
        await Record(new RecordExpenseCommand(new DateOnly(2026, 10, 3), "Fun", 100_000m, Note: "Dinner at restaurant"));

        var summary = await Save(OctoberPlan(netIncome: 11_000_000m, fun: 800_000m));

        Assert.Equal(11_000_000m, summary.NetIncome);
        var fun = summary.Jars.Single(jar => jar.Name == "Fun");
        Assert.Equal(800_000m, fun.Spending.Planned);
        Assert.Equal(100_000m, fun.Spending.Spent);
        Assert.Single(summary.Expenses);
    }

    [Fact]
    public async Task Replanning_away_a_jar_with_expenses_is_rejected()
    {
        await Save(OctoberPlan());
        await Record(new RecordExpenseCommand(new DateOnly(2026, 10, 3), "Fun", 100_000m));
        var withoutFun = OctoberPlan() with { Jars = [.. OctoberPlan().Jars.Where(jar => jar.Name != "Fun")] };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Save(withoutFun));

        Assert.Contains("Fun (1 expense)", exception.Message);
    }

    [Fact]
    public async Task Invalid_plans_are_rejected_by_the_domain_and_not_saved()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Save(OctoberPlan(netIncome: 5_000_000m)));

        Assert.Equal(0, _budgets.Saves);
    }

    [Fact]
    public async Task Records_a_dinner_from_Fun_without_a_planned_item()
    {
        await Save(OctoberPlan());

        var expense = await Record(new RecordExpenseCommand(new DateOnly(2026, 10, 3), "fun", 100_000m, Note: "Dinner at restaurant"));

        Assert.Equal(new ExpenseDto(new DateOnly(2026, 10, 3), "Fun", null, 100_000m, "Dinner at restaurant"), expense);
        Assert.Equal(2, _budgets.Saves);
    }

    [Fact]
    public async Task Recording_an_expense_needs_the_months_budget()
    {
        await Save(OctoberPlan());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Record(new RecordExpenseCommand(new DateOnly(2026, 11, 2), "Fun", 100_000m)));
    }

    [Fact]
    public async Task The_monthly_summary_shows_planned_vs_spent_per_jar_and_item()
    {
        await Save(OctoberPlan());
        await Record(new RecordExpenseCommand(new DateOnly(2026, 10, 12), "Fixed expenses", 180_000m, "Utilities", "Electricity"));
        await Record(new RecordExpenseCommand(new DateOnly(2026, 10, 1), "Fixed expenses", 2_800_000m, "Rent"));
        await Record(new RecordExpenseCommand(new DateOnly(2026, 10, 5), "Fun", 500_000m, Note: "Weekend trip"));
        await Record(new RecordExpenseCommand(new DateOnly(2026, 10, 20), "Fun", 220_000m, Note: "Birthday dinner"));

        var summary = await Summary(2026, 10);

        Assert.Equal(3_700_000m, summary.TotalSpent);

        var fixedExpenses = summary.Jars[0];
        Assert.Equal(new SpendingDto(3_250_000m, 2_980_000m, 270_000m, 2_980_000m / 3_250_000m * 100m), fixedExpenses.Spending);
        Assert.Equal(new SpendingDto(2_800_000m, 2_800_000m, 0m, 100m), fixedExpenses.Items[0].Spending);
        Assert.Equal(28m, fixedExpenses.Items[0].ShareOfIncome);

        var fun = summary.Jars[1];
        Assert.Equal(new SpendingDto(600_000m, 720_000m, -120_000m, 120m), fun.Spending);

        Assert.Equal(new SpendingDto(3_000_000m, 0m, 3_000_000m, 0m), summary.Jars[2].Spending);

        // Expenses are listed by date.
        Assert.Equal(
            [new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 12), new DateOnly(2026, 10, 20)],
            summary.Expenses.Select(expense => expense.Date));
    }

    [Fact]
    public async Task The_monthly_summary_of_a_month_without_budget_is_not_found()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Summary(2026, 10));
    }

    [Fact]
    public async Task The_draft_of_a_saved_month_is_its_budget()
    {
        await Save(OctoberPlan());

        var draft = await Draft(2026, 10);

        Assert.Equal(BudgetDraftSource.Saved, draft.Source);
        Assert.Null(draft.CopiedFrom);
        Assert.Equal(10_000_000m, draft.NetIncome);
        Assert.Equal(3, draft.Jars.Count);
        Assert.Equal(28m, draft.Jars[0].Items[0].ShareOfIncome);
    }

    [Fact]
    public async Task A_new_month_is_pre_filled_from_the_latest_earlier_month_without_saving_it()
    {
        await Save(OctoberPlan());
        await Record(new RecordExpenseCommand(new DateOnly(2026, 10, 3), "Fun", 100_000m));

        // December skips November: it is copied from October, the latest month with a budget.
        var draft = await Draft(2026, 12);

        Assert.Equal(BudgetDraftSource.CopiedFromEarlierMonth, draft.Source);
        Assert.Equal("2026-10", draft.CopiedFrom);
        Assert.Equal((2026, 12), (draft.Year, draft.Month));
        Assert.Equal(10_000_000m, draft.NetIncome);
        Assert.Equal(3_150_000m, draft.Unassigned);
        Assert.Equal(["Fixed expenses", "Fun", "Savings"], draft.Jars.Select(jar => jar.Name));
        await Assert.ThrowsAsync<NotFoundException>(() => Summary(2026, 12));
    }

    [Fact]
    public async Task The_first_month_ever_starts_empty()
    {
        var draft = await Draft(2026, 10);

        Assert.Equal(BudgetDraftSource.Empty, draft.Source);
        Assert.Null(draft.NetIncome);
        Assert.Null(draft.Unassigned);
        Assert.Equal("COP", draft.Currency);
        Assert.Empty(draft.Jars);
    }

    [Fact]
    public async Task Later_months_are_not_used_to_pre_fill_an_earlier_month()
    {
        await Save(OctoberPlan());

        var draft = await Draft(2026, 9);

        Assert.Equal(BudgetDraftSource.Empty, draft.Source);
    }

    [Fact]
    public async Task Suggests_Six_Jars_amounts_for_the_income()
    {
        var suggestions = await new SuggestSixJarsHandler().HandleAsync(new SuggestSixJarsQuery(10_000_000m), None);

        Assert.Equal(
            [
                new SuggestedJarDto("necessities", 55m, 5_500_000m),
                new SuggestedJarDto("financial-freedom", 10m, 1_000_000m),
                new SuggestedJarDto("long-term-savings", 10m, 1_000_000m),
                new SuggestedJarDto("education", 10m, 1_000_000m),
                new SuggestedJarDto("play", 10m, 1_000_000m),
                new SuggestedJarDto("give", 5m, 500_000m),
            ],
            suggestions);
    }
}
