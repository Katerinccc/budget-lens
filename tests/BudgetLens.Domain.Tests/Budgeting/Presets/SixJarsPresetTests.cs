using BudgetLens.Domain.Budgeting;
using BudgetLens.Domain.Budgeting.Presets;
using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Tests.Budgeting.Presets;

public class SixJarsPresetTests
{
    private static readonly BudgetMonth October = new(2026, 10);

    [Fact]
    public void Suggests_the_six_jars_percentages_adding_up_to_100()
    {
        Assert.Equal(
            ["necessities", "financial-freedom", "long-term-savings", "education", "play", "give"],
            SixJarsPreset.Suggestions.Select(suggestion => suggestion.Key));
        Assert.Equal([55m, 10m, 10m, 10m, 10m, 5m], SixJarsPreset.Suggestions.Select(suggestion => suggestion.Share.Value));
        Assert.Equal(100m, SixJarsPreset.Suggestions.Sum(suggestion => suggestion.Share.Value));
    }

    [Fact]
    public void Suggests_amounts_for_the_income()
    {
        var amounts = SixJarsPreset.SuggestAmounts(Money.Cop(10_000_000m));

        Assert.Equal(
            [5_500_000m, 1_000_000m, 1_000_000m, 1_000_000m, 1_000_000m, 500_000m],
            amounts.Select(suggested => suggested.Amount.Amount));
        Assert.Equal("necessities", amounts[0].Key);
        Assert.Equal(new Percentage(55m), amounts[0].Share);
    }

    [Theory]
    [InlineData(3_333_333)]
    [InlineData(1_000_001)]
    [InlineData(7)]
    public void Suggested_amounts_are_whole_pesos_adding_up_to_the_income(decimal income)
    {
        var amounts = SixJarsPreset.SuggestAmounts(Money.Cop(income));

        Assert.Equal(income, amounts.Sum(suggested => suggested.Amount.Amount));
        Assert.All(amounts, suggested => Assert.Equal(decimal.Truncate(suggested.Amount.Amount), suggested.Amount.Amount));
    }

    [Fact]
    public void Accepting_the_suggestions_matches_the_preset_exactly()
    {
        var jars = SixJarsPreset.SuggestAmounts(Money.Cop(10_000_000m))
            .Select(suggested => Jar.Single(suggested.Key, suggested.Amount, suggested.Key));
        var budget = new MonthlyBudget(October, Money.Cop(10_000_000m), jars);

        var comparison = SixJarsPreset.Compare(budget);

        Assert.All(comparison.Suggestions, suggestion => Assert.Equal(0m, suggestion.DifferenceInPoints));
        Assert.Equal(Money.Cop(0m), budget.Unassigned);
    }

    [Fact]
    public void Compares_the_users_own_amounts_with_the_suggestions()
    {
        // The user's real distribution of 10.000.000, with jars renamed in Spanish.
        var budget = new MonthlyBudget(October, Money.Cop(10_000_000m),
        [
            new Jar("Gastos fijos",
            [
                new PlannedItem("Arriendo", Money.Cop(2_800_000m)),
                new PlannedItem("Deudas", Money.Cop(1_800_000m)),
                new PlannedItem("Mercado", Money.Cop(650_000m)),
                new PlannedItem("Servicios públicos", Money.Cop(450_000m)),
                new PlannedItem("Gimnasio", Money.Cop(180_000m)),
                new PlannedItem("Celular", Money.Cop(120_000m)),
            ], "necessities"),
            Jar.Single("Ahorro", Money.Cop(3_000_000m), "financial-freedom"),
            Jar.Single("Diversión", Money.Cop(600_000m), "play"),
            Jar.Single("Educación", Money.Cop(200_000m), "education"),
            Jar.Single("Donaciones", Money.Cop(200_000m), "give"),
        ]);

        var comparison = SixJarsPreset.Compare(budget);

        Assert.Equal(
            [
                ("necessities", 55m, 60m, 5m),
                ("financial-freedom", 10m, 30m, 20m),
                ("long-term-savings", 10m, 0m, -10m), // no jar linked: counts as 0%
                ("education", 10m, 2m, -8m),
                ("play", 10m, 6m, -4m),
                ("give", 5m, 2m, -3m),
            ],
            comparison.Suggestions.Select(s => (s.Key, s.Suggested.Value, s.Actual.Value, s.DifferenceInPoints)));
        Assert.Empty(comparison.OtherJars);
    }

    [Fact]
    public void Jars_linked_to_the_same_suggestion_are_added_together()
    {
        var budget = new MonthlyBudget(October, Money.Cop(10_000_000m),
        [
            Jar.Single("Emergency fund", Money.Cop(500_000m), "long-term-savings"),
            Jar.Single("Retirement", Money.Cop(700_000m), "long-term-savings"),
        ]);

        var longTermSavings = SixJarsPreset.Compare(budget).Suggestions.Single(s => s.Key == "long-term-savings");

        Assert.Equal(new Percentage(12m), longTermSavings.Actual);
        Assert.Equal(2m, longTermSavings.DifferenceInPoints);
    }

    [Fact]
    public void Custom_jars_are_listed_apart_and_not_counted()
    {
        var pets = Jar.Single("Pets", Money.Cop(300_000m));
        var travel = Jar.Single("Travel", Money.Cop(400_000m), "some-other-preset-key");
        var budget = new MonthlyBudget(October, Money.Cop(10_000_000m), [pets, travel]);

        var comparison = SixJarsPreset.Compare(budget);

        Assert.Equal([pets, travel], comparison.OtherJars);
        Assert.All(comparison.Suggestions, suggestion => Assert.Equal(new Percentage(0m), suggestion.Actual));
    }

    [Fact]
    public void Suggestions_need_a_whole_peso_income()
    {
        Assert.Throws<InvalidOperationException>(() => SixJarsPreset.SuggestAmounts(Money.Cop(1_000_000.5m)));
    }
}
