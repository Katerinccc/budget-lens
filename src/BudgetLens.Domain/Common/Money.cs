namespace BudgetLens.Domain.Common;

/// <summary>
/// An amount in a currency. Amounts are <see cref="decimal"/> and are never rounded implicitly:
/// callers choose when and how to round.
/// </summary>
public sealed record Money
{
    public const string DefaultCurrency = "COP";

    public decimal Amount { get; }

    /// <summary>ISO 4217 code, e.g. "COP".</summary>
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (currency is not { Length: 3 } || !currency.All(char.IsAsciiLetterUpper))
        {
            throw new ArgumentException($"'{currency}' is not a three-letter uppercase ISO 4217 code.", nameof(currency));
        }

        Amount = amount;
        Currency = currency;
    }

    public static Money Cop(decimal amount) => new(amount, DefaultCurrency);

    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public Money RoundToWholeUnits(MidpointRounding rule) =>
        new(decimal.Round(Amount, 0, rule), Currency);

    /// <summary>
    /// Splits this amount into whole units (e.g. whole pesos) by the given shares, which must add up
    /// to 100%. Each part is floored first; the units left over go one by one to the parts with the
    /// largest remainders (ties go to the earlier share), so the parts always add up to this amount.
    /// </summary>
    public IReadOnlyList<Money> AllocateWholeUnits(IReadOnlyList<Percentage> shares)
    {
        if (shares.Count == 0)
        {
            throw new ArgumentException("At least one share is required.", nameof(shares));
        }

        if (shares.Sum(share => share.Value) != 100m)
        {
            throw new ArgumentException("Shares must add up to exactly 100%.", nameof(shares));
        }

        if (Amount < 0m || Amount != decimal.Truncate(Amount))
        {
            throw new InvalidOperationException("Only non-negative whole amounts can be allocated in whole units.");
        }

        var exact = shares.Select(share => share.ApplyTo(this).Amount).ToArray();
        var parts = exact.Select(decimal.Floor).ToArray();
        var leftoverUnits = (int)(Amount - parts.Sum());

        var byLargestRemainder = Enumerable.Range(0, exact.Length)
            .OrderByDescending(i => exact[i] - parts[i])
            .ThenBy(i => i);

        foreach (var i in byLargestRemainder.Take(leftoverUnits))
        {
            parts[i] += 1m;
        }

        return [.. parts.Select(part => new Money(part, Currency))];
    }

    internal static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Cannot combine {left.Currency} with {right.Currency}.");
        }
    }

    public override string ToString() => $"{Amount} {Currency}";
}
