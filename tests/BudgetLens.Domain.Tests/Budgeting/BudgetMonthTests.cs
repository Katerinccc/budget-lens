using BudgetLens.Domain.Budgeting;

namespace BudgetLens.Domain.Tests.Budgeting;

public class BudgetMonthTests
{
    [Fact]
    public void Holds_a_year_and_month()
    {
        var month = new BudgetMonth(2026, 10);

        Assert.Equal(2026, month.Year);
        Assert.Equal(10, month.Month);
        Assert.Equal("2026-10", month.ToString());
    }

    [Theory]
    [InlineData(2026, 0)]
    [InlineData(2026, 13)]
    [InlineData(0, 1)]
    [InlineData(10_000, 1)]
    public void Rejects_invalid_months(int year, int month)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BudgetMonth(year, month));
    }

    [Fact]
    public void Orders_months_chronologically()
    {
        Assert.True(new BudgetMonth(2026, 12).CompareTo(new BudgetMonth(2027, 1)) < 0);
        Assert.True(new BudgetMonth(2026, 11).CompareTo(new BudgetMonth(2026, 10)) > 0);
        Assert.Equal(0, new BudgetMonth(2026, 10).CompareTo(new BudgetMonth(2026, 10)));
    }
}
