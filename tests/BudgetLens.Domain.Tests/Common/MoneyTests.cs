using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Tests.Common;

public class MoneyTests
{
    [Fact]
    public void Cop_creates_money_in_Colombian_pesos()
    {
        var money = Money.Cop(1_500_000m);

        Assert.Equal(1_500_000m, money.Amount);
        Assert.Equal("COP", money.Currency);
    }

    [Theory]
    [InlineData("")]
    [InlineData("cop")]
    [InlineData("CO")]
    [InlineData("COPS")]
    [InlineData("C0P")]
    public void Currency_must_be_a_three_letter_uppercase_ISO_code(string currency)
    {
        Assert.Throws<ArgumentException>(() => new Money(100m, currency));
    }

    [Fact]
    public void Money_with_the_same_amount_and_currency_is_equal()
    {
        Assert.Equal(Money.Cop(1_000m), Money.Cop(1_000.00m));
        Assert.NotEqual(Money.Cop(1_000m), new Money(1_000m, "USD"));
    }

    [Fact]
    public void Adds_and_subtracts_money_in_the_same_currency()
    {
        var income = Money.Cop(3_500_000m);
        var rent = Money.Cop(1_200_000m);

        Assert.Equal(Money.Cop(4_700_000m), income + rent);
        Assert.Equal(Money.Cop(2_300_000m), income - rent);
    }

    [Fact]
    public void Subtraction_can_go_below_zero()
    {
        Assert.Equal(Money.Cop(-50_000m), Money.Cop(100_000m) - Money.Cop(150_000m));
    }

    [Fact]
    public void Mixing_currencies_is_rejected()
    {
        var pesos = Money.Cop(1_000m);
        var dollars = new Money(1_000m, "USD");

        Assert.Throws<InvalidOperationException>(() => pesos + dollars);
        Assert.Throws<InvalidOperationException>(() => pesos - dollars);
    }

    [Theory]
    [InlineData(1_234.5, MidpointRounding.AwayFromZero, 1_235)]
    [InlineData(1_234.5, MidpointRounding.ToEven, 1_234)]
    [InlineData(1_234.49, MidpointRounding.AwayFromZero, 1_234)]
    [InlineData(-1_234.5, MidpointRounding.AwayFromZero, -1_235)]
    public void Rounds_to_whole_units_with_an_explicit_rule(decimal amount, MidpointRounding rule, decimal expected)
    {
        Assert.Equal(Money.Cop(expected), Money.Cop(amount).RoundToWholeUnits(rule));
    }

    [Fact]
    public void Allocates_in_whole_pesos_giving_leftover_pesos_to_the_largest_remainders()
    {
        // 1.000 split 33,33 / 33,33 / 33,34 is 333,3 / 333,3 / 333,4: one peso is left over
        // after flooring, and it goes to the share with the largest remainder (the third one).
        var shares = new[] { new Percentage(33.33m), new Percentage(33.33m), new Percentage(33.34m) };

        var parts = Money.Cop(1_000m).AllocateWholeUnits(shares);

        Assert.Equal([Money.Cop(333m), Money.Cop(333m), Money.Cop(334m)], parts);
    }

    [Fact]
    public void Allocation_ties_go_to_the_earlier_share()
    {
        var shares = new[] { new Percentage(50m), new Percentage(50m) };

        var parts = Money.Cop(1_001m).AllocateWholeUnits(shares);

        Assert.Equal([Money.Cop(501m), Money.Cop(500m)], parts);
    }

    [Theory]
    [InlineData(3_500_000)]
    [InlineData(3_333_333)]
    [InlineData(1_000_001)]
    [InlineData(7)]
    [InlineData(0)]
    public void Allocated_parts_always_add_up_to_the_original_amount(decimal amount)
    {
        // Six jars preset: 55 / 10 / 10 / 10 / 10 / 5.
        var shares = new[] { 55m, 10m, 10m, 10m, 10m, 5m }.Select(p => new Percentage(p)).ToList();

        var parts = Money.Cop(amount).AllocateWholeUnits(shares);

        Assert.Equal(Money.Cop(amount), parts.Aggregate(Money.Cop(0m), (sum, part) => sum + part));
        Assert.All(parts, part => Assert.Equal(decimal.Truncate(part.Amount), part.Amount));
    }

    [Fact]
    public void Allocation_requires_shares_that_add_up_to_100_percent()
    {
        var shares = new[] { new Percentage(50m), new Percentage(40m) };

        Assert.Throws<ArgumentException>(() => Money.Cop(1_000m).AllocateWholeUnits(shares));
    }

    [Fact]
    public void Allocation_requires_at_least_one_share()
    {
        Assert.Throws<ArgumentException>(() => Money.Cop(1_000m).AllocateWholeUnits([]));
    }

    [Theory]
    [InlineData(1_000.5)]
    [InlineData(-1_000)]
    public void Only_non_negative_whole_amounts_can_be_allocated(decimal amount)
    {
        var shares = new[] { new Percentage(100m) };

        Assert.Throws<InvalidOperationException>(() => Money.Cop(amount).AllocateWholeUnits(shares));
    }
}
