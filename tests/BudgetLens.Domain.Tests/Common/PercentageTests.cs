using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Tests.Common;

public class PercentageTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(12.5)]
    [InlineData(100)]
    public void Accepts_values_from_0_to_100(decimal value)
    {
        Assert.Equal(value, new Percentage(value).Value);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void Rejects_values_outside_0_to_100(decimal value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Percentage(value));
    }

    [Fact]
    public void Applies_to_money_without_rounding()
    {
        var income = Money.Cop(3_500_000m);

        Assert.Equal(Money.Cop(1_925_000m), new Percentage(55m).ApplyTo(income));
        Assert.Equal(Money.Cop(1_166_655m), new Percentage(33.333m).ApplyTo(income));
    }

    [Fact]
    public void Computes_which_percentage_a_part_is_of_a_whole()
    {
        // A user types the amount for a jar; the app derives its percentage of the income.
        var percentage = Percentage.Of(Money.Cop(1_750_000m), Money.Cop(3_500_000m));

        Assert.Equal(new Percentage(50m), percentage);
    }

    [Fact]
    public void Keeps_full_precision_when_the_percentage_is_not_exact()
    {
        var percentage = Percentage.Of(Money.Cop(1_000_000m), Money.Cop(3_000_000m));

        Assert.Equal(33.33m, decimal.Round(percentage.Value, 2));
        Assert.Equal(Money.Cop(1_000_000m), percentage.ApplyTo(Money.Cop(3_000_000m)).RoundToWholeUnits(MidpointRounding.AwayFromZero));
    }

    [Fact]
    public void A_part_cannot_be_more_than_the_whole()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Percentage.Of(Money.Cop(4_000_000m), Money.Cop(3_500_000m)));
    }

    [Fact]
    public void The_whole_must_be_greater_than_zero()
    {
        Assert.Throws<ArgumentException>(() => Percentage.Of(Money.Cop(0m), Money.Cop(0m)));
    }

    [Fact]
    public void Part_and_whole_must_use_the_same_currency()
    {
        Assert.Throws<InvalidOperationException>(() => Percentage.Of(Money.Cop(1_000m), new Money(2_000m, "USD")));
    }
}
