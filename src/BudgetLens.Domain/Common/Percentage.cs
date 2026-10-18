namespace BudgetLens.Domain.Common;

/// <summary>A percentage between 0 and 100 (inclusive), kept at full <see cref="decimal"/> precision.</summary>
public readonly record struct Percentage
{
    public decimal Value { get; }

    public Percentage(decimal value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 0m);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 100m);
        Value = value;
    }

    /// <summary>The percentage that <paramref name="part"/> represents of <paramref name="whole"/>.</summary>
    public static Percentage Of(Money part, Money whole)
    {
        Money.EnsureSameCurrency(part, whole);

        if (whole.Amount <= 0m)
        {
            throw new ArgumentException("The whole must be greater than zero.", nameof(whole));
        }

        return new Percentage(part.Amount / whole.Amount * 100m);
    }

    /// <summary>This percentage of <paramref name="money"/>, not rounded.</summary>
    public Money ApplyTo(Money money) => new(money.Amount * Value / 100m, money.Currency);

    public override string ToString() => $"{Value}%";
}
