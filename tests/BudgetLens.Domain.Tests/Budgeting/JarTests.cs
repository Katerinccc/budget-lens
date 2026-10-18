using BudgetLens.Domain.Budgeting;
using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Tests.Budgeting;

public class JarTests
{
    [Fact]
    public void Planned_amount_is_the_sum_of_its_items()
    {
        var fixedExpenses = new Jar("Fixed expenses",
        [
            new PlannedItem("Rent", Money.Cop(2_800_000m)),
            new PlannedItem("Utilities", Money.Cop(450_000m)),
            new PlannedItem("Gym", Money.Cop(180_000m)),
        ]);

        Assert.Equal(Money.Cop(3_430_000m), fixedExpenses.Planned);
        Assert.Equal(3, fixedExpenses.Items.Count);
    }

    [Fact]
    public void A_single_amount_jar_has_one_item_with_the_jar_name()
    {
        var fun = Jar.Single("Fun", Money.Cop(600_000m));

        var item = Assert.Single(fun.Items);
        Assert.Equal("Fun", item.Name);
        Assert.Equal(Money.Cop(600_000m), fun.Planned);
    }

    [Fact]
    public void Names_are_trimmed()
    {
        var jar = new Jar("  Fun ", [new PlannedItem(" Cinema  ", Money.Cop(50_000m))]);

        Assert.Equal("Fun", jar.Name);
        Assert.Equal("Cinema", jar.Items[0].Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Jar_and_item_names_are_required(string name)
    {
        Assert.Throws<ArgumentException>(() => Jar.Single(name, Money.Cop(1_000m)));
        Assert.Throws<ArgumentException>(() => new PlannedItem(name, Money.Cop(1_000m)));
    }

    [Fact]
    public void A_jar_needs_at_least_one_item()
    {
        Assert.Throws<ArgumentException>(() => new Jar("Fun", []));
    }

    [Fact]
    public void Item_names_are_unique_ignoring_case()
    {
        Assert.Throws<ArgumentException>(() => new Jar("Fixed expenses",
        [
            new PlannedItem("Rent", Money.Cop(1_000m)),
            new PlannedItem("rent", Money.Cop(2_000m)),
        ]));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void A_planned_amount_must_be_greater_than_zero(decimal amount)
    {
        Assert.Throws<ArgumentException>(() => new PlannedItem("Rent", Money.Cop(amount)));
        Assert.Throws<ArgumentException>(() => Jar.Single("Donations", Money.Cop(amount)));
    }

    [Fact]
    public void A_jar_is_custom_unless_linked_to_a_suggestion()
    {
        Assert.Null(Jar.Single("Pets", Money.Cop(100_000m)).SuggestionKey);
        Assert.Equal("necessities", Jar.Single("Gastos fijos", Money.Cop(100_000m), "necessities").SuggestionKey);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void A_suggestion_key_cannot_be_blank(string key)
    {
        Assert.Throws<ArgumentException>(() => Jar.Single("Fun", Money.Cop(1_000m), key));
    }

    [Fact]
    public void Items_must_share_a_currency()
    {
        Assert.Throws<InvalidOperationException>(() => new Jar("Fixed expenses",
        [
            new PlannedItem("Rent", Money.Cop(1_000m)),
            new PlannedItem("Streaming", new Money(10m, "USD")),
        ]));
    }
}
