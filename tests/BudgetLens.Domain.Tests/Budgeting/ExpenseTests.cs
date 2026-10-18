using BudgetLens.Domain.Budgeting;
using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Tests.Budgeting;

public class ExpenseTests
{
    private static readonly BudgetMonth October = new(2026, 10);

    private static MonthlyBudget OctoberBudget() => new(October, Money.Cop(10_000_000m),
    [
        new Jar("Fixed expenses",
        [
            new PlannedItem("Rent", Money.Cop(2_800_000m)),
            new PlannedItem("Utilities", Money.Cop(450_000m)),
        ]),
        Jar.Single("Fun", Money.Cop(600_000m)),
        Jar.Single("Savings", Money.Cop(3_000_000m)),
    ]);

    [Fact]
    public void Records_spending_from_a_jar_without_a_planned_item()
    {
        var budget = OctoberBudget();

        var dinner = budget.RecordExpense(new DateOnly(2026, 10, 3), "Fun", Money.Cop(100_000m), note: "Dinner at restaurant");

        Assert.Equal("Fun", dinner.JarName);
        Assert.Null(dinner.ItemName);
        Assert.Equal("Dinner at restaurant", dinner.Note);
        Assert.Equal(Money.Cop(100_000m), budget.StatusOf("Fun").Spent);
    }

    [Fact]
    public void Records_an_expense_against_a_planned_item()
    {
        var budget = OctoberBudget();

        var rent = budget.RecordExpense(new DateOnly(2026, 10, 1), "Fixed expenses", Money.Cop(2_800_000m), "Rent");

        Assert.Equal("Rent", rent.ItemName);
        Assert.Null(rent.Note);
        Assert.Equal(100m, budget.StatusOf("Fixed expenses", "Rent").UsedPercent);
    }

    [Fact]
    public void Tracks_a_realistic_month_per_jar_and_item()
    {
        var budget = OctoberBudget();
        budget.RecordExpense(new DateOnly(2026, 10, 1), "Fixed expenses", Money.Cop(2_800_000m), "Rent");
        budget.RecordExpense(new DateOnly(2026, 10, 12), "Fixed expenses", Money.Cop(180_000m), "Utilities", "Electricity");
        budget.RecordExpense(new DateOnly(2026, 10, 14), "Fixed expenses", Money.Cop(95_000m), "Utilities", "Water");
        budget.RecordExpense(new DateOnly(2026, 10, 3), "Fun", Money.Cop(100_000m), note: "Dinner at restaurant");
        budget.RecordExpense(new DateOnly(2026, 10, 18), "Fun", Money.Cop(350_000m), note: "Concert");

        var fixedExpenses = budget.StatusOf("Fixed expenses");
        Assert.Equal(Money.Cop(3_250_000m), fixedExpenses.Planned);
        Assert.Equal(Money.Cop(3_075_000m), fixedExpenses.Spent);
        Assert.Equal(Money.Cop(175_000m), fixedExpenses.Remaining);

        var utilities = budget.StatusOf("Fixed expenses", "Utilities");
        Assert.Equal(Money.Cop(275_000m), utilities.Spent);
        Assert.Equal(Money.Cop(175_000m), utilities.Remaining);

        var fun = budget.StatusOf("Fun");
        Assert.Equal(Money.Cop(150_000m), fun.Remaining);
        Assert.Equal(75m, fun.UsedPercent);

        var savings = budget.StatusOf("Savings");
        Assert.Equal(Money.Cop(0m), savings.Spent);
        Assert.Equal(0m, savings.UsedPercent);

        Assert.Equal(Money.Cop(3_525_000m), budget.TotalSpent);
        Assert.Equal(5, budget.Expenses.Count);
    }

    [Fact]
    public void Spending_over_the_plan_is_allowed_and_reported()
    {
        var budget = OctoberBudget();
        budget.RecordExpense(new DateOnly(2026, 10, 5), "Fun", Money.Cop(500_000m), note: "Weekend trip");
        budget.RecordExpense(new DateOnly(2026, 10, 20), "Fun", Money.Cop(220_000m), note: "Birthday dinner");

        var fun = budget.StatusOf("Fun");

        Assert.Equal(Money.Cop(-120_000m), fun.Remaining);
        Assert.Equal(120m, fun.UsedPercent);
    }

    [Fact]
    public void Jar_and_item_are_found_ignoring_case_and_stored_with_their_planned_names()
    {
        var budget = OctoberBudget();

        var expense = budget.RecordExpense(new DateOnly(2026, 10, 1), " fixed EXPENSES ", Money.Cop(2_800_000m), "rent");

        Assert.Equal("Fixed expenses", expense.JarName);
        Assert.Equal("Rent", expense.ItemName);
        Assert.Equal(Money.Cop(2_800_000m), budget.StatusOf("FIXED EXPENSES", "RENT").Spent);
    }

    [Fact]
    public void An_expense_needs_an_existing_jar()
    {
        Assert.Throws<ArgumentException>(() => OctoberBudget().RecordExpense(new DateOnly(2026, 10, 3), "Travel", Money.Cop(100_000m)));
    }

    [Fact]
    public void The_item_must_belong_to_the_jar()
    {
        Assert.Throws<ArgumentException>(() => OctoberBudget().RecordExpense(new DateOnly(2026, 10, 3), "Fun", Money.Cop(100_000m), "Rent"));
    }

    [Theory]
    [InlineData(2026, 9, 30)]
    [InlineData(2026, 11, 1)]
    [InlineData(2025, 10, 15)]
    public void The_date_must_be_in_the_budget_month(int year, int month, int day)
    {
        Assert.Throws<ArgumentException>(() => OctoberBudget().RecordExpense(new DateOnly(year, month, day), "Fun", Money.Cop(100_000m)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void An_expense_must_be_greater_than_zero(decimal amount)
    {
        Assert.Throws<ArgumentException>(() => OctoberBudget().RecordExpense(new DateOnly(2026, 10, 3), "Fun", Money.Cop(amount)));
    }

    [Fact]
    public void An_expense_must_use_the_income_currency()
    {
        Assert.Throws<InvalidOperationException>(() => OctoberBudget().RecordExpense(new DateOnly(2026, 10, 3), "Fun", new Money(30m, "USD")));
    }

    [Fact]
    public void A_blank_note_is_stored_as_no_note()
    {
        var expense = OctoberBudget().RecordExpense(new DateOnly(2026, 10, 3), "Fun", Money.Cop(100_000m), note: "   ");

        Assert.Null(expense.Note);
    }

    [Fact]
    public void Copying_to_the_next_month_starts_without_expenses()
    {
        var october = OctoberBudget();
        october.RecordExpense(new DateOnly(2026, 10, 3), "Fun", Money.Cop(100_000m));

        var november = october.CopyTo(new BudgetMonth(2026, 11));

        Assert.Empty(november.Expenses);
        Assert.Equal(Money.Cop(0m), november.StatusOf("Fun").Spent);
        Assert.Single(october.Expenses);
    }

    [Fact]
    public void A_month_knows_which_dates_it_contains()
    {
        Assert.True(October.Contains(new DateOnly(2026, 10, 31)));
        Assert.False(October.Contains(new DateOnly(2026, 11, 1)));
    }
}
