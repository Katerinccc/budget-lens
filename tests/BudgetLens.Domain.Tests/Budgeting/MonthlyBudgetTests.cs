using BudgetLens.Domain.Budgeting;
using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Tests.Budgeting;

public class MonthlyBudgetTests
{
    private static readonly BudgetMonth October = new(2026, 10);

    private static Jar FixedExpenses() => new("Fixed expenses",
    [
        new PlannedItem("Rent", Money.Cop(2_800_000m)),
        new PlannedItem("Debt payment", Money.Cop(1_800_000m)),
        new PlannedItem("Groceries", Money.Cop(650_000m)),
        new PlannedItem("Utilities", Money.Cop(450_000m)),
        new PlannedItem("Gym", Money.Cop(180_000m)),
        new PlannedItem("Cellphone", Money.Cop(120_000m)),
    ]);

    [Fact]
    public void Derives_each_jar_percentage_from_the_typed_amounts()
    {
        var budget = new MonthlyBudget(October, Money.Cop(10_000_000m),
        [
            FixedExpenses(),
            Jar.Single("Savings", Money.Cop(3_000_000m)),
            Jar.Single("Fun", Money.Cop(600_000m)),
            Jar.Single("Education", Money.Cop(200_000m)),
            Jar.Single("Donations", Money.Cop(200_000m)),
        ]);

        Assert.Equal(
            [60m, 30m, 6m, 2m, 2m],
            budget.Jars.Select(jar => budget.ShareOfIncome(jar.Planned).Value));
        Assert.Equal(Money.Cop(10_000_000m), budget.Planned);
        Assert.Equal(Money.Cop(0m), budget.Unassigned);
    }

    [Fact]
    public void Derives_the_percentage_of_a_single_item()
    {
        var budget = new MonthlyBudget(October, Money.Cop(10_000_000m), [FixedExpenses()]);

        Assert.Equal(new Percentage(28m), budget.ShareOfIncome(budget.Jars[0].Items[0].Amount));
    }

    [Fact]
    public void Whatever_is_not_planned_is_unassigned()
    {
        // No savings jar: that is allowed, the rest simply stays unassigned.
        var budget = new MonthlyBudget(October, Money.Cop(10_000_000m), [FixedExpenses(), Jar.Single("Fun", Money.Cop(600_000m))]);

        Assert.Equal(Money.Cop(3_400_000m), budget.Unassigned);
        Assert.Equal(new Percentage(34m), budget.ShareOfIncome(budget.Unassigned));
    }

    [Fact]
    public void A_budget_can_start_with_only_the_income()
    {
        var budget = new MonthlyBudget(October, Money.Cop(10_000_000m), []);

        Assert.Empty(budget.Jars);
        Assert.Equal(Money.Cop(10_000_000m), budget.Unassigned);
    }

    [Fact]
    public void Percentages_that_do_not_divide_evenly_keep_full_precision()
    {
        var budget = new MonthlyBudget(October, Money.Cop(3_000_000m), [Jar.Single("Rent", Money.Cop(1_000_000m))]);

        Assert.Equal(33.33m, decimal.Round(budget.ShareOfIncome(budget.Jars[0].Planned).Value, 2));
    }

    [Fact]
    public void Planning_more_than_the_income_is_rejected_with_the_excess()
    {
        var exception = Assert.Throws<ArgumentException>(() => new MonthlyBudget(October, Money.Cop(5_000_000m), [FixedExpenses()]));

        Assert.Contains("1000000 COP", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void The_net_income_must_be_greater_than_zero(decimal income)
    {
        Assert.Throws<ArgumentException>(() => new MonthlyBudget(October, Money.Cop(income), []));
    }

    [Fact]
    public void Jar_names_are_unique_ignoring_case()
    {
        Assert.Throws<ArgumentException>(() => new MonthlyBudget(October, Money.Cop(1_000_000m),
        [
            Jar.Single("Fun", Money.Cop(100_000m)),
            Jar.Single("FUN", Money.Cop(100_000m)),
        ]));
    }

    [Fact]
    public void Jars_must_use_the_income_currency()
    {
        Assert.Throws<InvalidOperationException>(() => new MonthlyBudget(October, Money.Cop(1_000_000m),
        [
            Jar.Single("Travel", new Money(100m, "USD")),
        ]));
    }

    [Fact]
    public void Copies_income_and_jars_to_a_later_month()
    {
        var october = new MonthlyBudget(October, Money.Cop(10_000_000m), [FixedExpenses()]);

        var november = october.CopyTo(new BudgetMonth(2026, 11));

        Assert.Equal(new BudgetMonth(2026, 11), november.Month);
        Assert.Equal(october.NetIncome, november.NetIncome);
        Assert.Equal(october.Jars, november.Jars);
        Assert.Equal(october.Unassigned, november.Unassigned);
    }

    [Theory]
    [InlineData(2026, 10)]
    [InlineData(2026, 9)]
    public void Can_only_copy_to_a_later_month(int year, int month)
    {
        var october = new MonthlyBudget(October, Money.Cop(10_000_000m), []);

        Assert.Throws<ArgumentException>(() => october.CopyTo(new BudgetMonth(year, month)));
    }
}
