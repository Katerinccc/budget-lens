using BudgetLens.Domain.Budgeting;
using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Tests.Budgeting;

public class ReplanTests
{
    private static readonly BudgetMonth October = new(2026, 10);

    private static Jar FixedExpenses(decimal rent = 2_800_000m) => new("Fixed expenses",
    [
        new PlannedItem("Rent", Money.Cop(rent)),
        new PlannedItem("Utilities", Money.Cop(450_000m)),
    ]);

    private static MonthlyBudget OctoberWithExpenses()
    {
        var budget = new MonthlyBudget(October, Money.Cop(10_000_000m), [FixedExpenses(), Jar.Single("Fun", Money.Cop(600_000m))]);
        budget.RecordExpense(new DateOnly(2026, 10, 1), "Fixed expenses", Money.Cop(2_800_000m), "Rent");
        budget.RecordExpense(new DateOnly(2026, 10, 3), "Fun", Money.Cop(100_000m), note: "Dinner at restaurant");
        budget.RecordExpense(new DateOnly(2026, 10, 18), "Fun", Money.Cop(350_000m), note: "Concert");
        return budget;
    }

    [Fact]
    public void Replaces_income_and_jars_and_keeps_the_expenses()
    {
        var budget = OctoberWithExpenses();

        budget.Replan(Money.Cop(11_000_000m),
        [
            FixedExpenses(rent: 3_000_000m),
            Jar.Single("Fun", Money.Cop(800_000m)),
            Jar.Single("Savings", Money.Cop(3_000_000m)),
        ]);

        Assert.Equal(Money.Cop(11_000_000m), budget.NetIncome);
        Assert.Equal(3, budget.Jars.Count);
        Assert.Equal(Money.Cop(7_250_000m), budget.Planned);
        Assert.Equal(Money.Cop(3_750_000m), budget.Unassigned);
        Assert.Equal(3, budget.Expenses.Count);
        Assert.Equal(Money.Cop(450_000m), budget.StatusOf("Fun").Spent);
        Assert.Equal(Money.Cop(200_000m), budget.StatusOf("Fixed expenses", "Rent").Remaining);
    }

    [Fact]
    public void Removing_a_jar_with_expenses_is_rejected_and_lists_them()
    {
        var budget = OctoberWithExpenses();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            budget.Replan(Money.Cop(10_000_000m), [FixedExpenses()]));

        Assert.Contains("Fun (2 expenses)", exception.Message);
        Assert.Equal(2, budget.Jars.Count); // unchanged
    }

    [Fact]
    public void Renaming_a_planned_item_with_expenses_is_rejected()
    {
        var budget = OctoberWithExpenses();
        var renamedRent = new Jar("Fixed expenses",
        [
            new PlannedItem("Apartment", Money.Cop(2_800_000m)),
            new PlannedItem("Utilities", Money.Cop(450_000m)),
        ]);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            budget.Replan(Money.Cop(10_000_000m), [renamedRent, Jar.Single("Fun", Money.Cop(600_000m))]));

        Assert.Contains("Fixed expenses / Rent (1 expense)", exception.Message);
    }

    [Fact]
    public void Changing_only_the_case_of_a_name_keeps_the_expenses_linked()
    {
        var budget = OctoberWithExpenses();

        budget.Replan(Money.Cop(10_000_000m), [FixedExpenses(), Jar.Single("FUN", Money.Cop(600_000m))]);

        Assert.Equal(Money.Cop(450_000m), budget.StatusOf("FUN").Spent);
    }

    [Fact]
    public void Removing_a_jar_without_expenses_is_allowed()
    {
        var budget = new MonthlyBudget(October, Money.Cop(10_000_000m), [FixedExpenses(), Jar.Single("Savings", Money.Cop(3_000_000m))]);
        budget.RecordExpense(new DateOnly(2026, 10, 1), "Fixed expenses", Money.Cop(2_800_000m), "Rent");

        budget.Replan(Money.Cop(10_000_000m), [FixedExpenses()]);

        Assert.Single(budget.Jars);
    }

    [Fact]
    public void The_new_plan_follows_the_same_rules_as_a_new_budget()
    {
        var budget = OctoberWithExpenses();

        Assert.Throws<ArgumentException>(() => budget.Replan(Money.Cop(1_000_000m), [FixedExpenses()]));
        Assert.Throws<ArgumentException>(() => budget.Replan(Money.Cop(0m), []));
        Assert.Equal(Money.Cop(10_000_000m), budget.NetIncome); // unchanged
    }

    [Fact]
    public void Changing_the_currency_with_expenses_is_rejected()
    {
        var budget = OctoberWithExpenses();

        Assert.Throws<InvalidOperationException>(() => budget.Replan(new Money(3_000m, "USD"), []));
    }
}
