namespace BudgetLens.Domain.Budgeting;

/// <summary>A calendar month (year and month) that a budget belongs to.</summary>
public readonly record struct BudgetMonth : IComparable<BudgetMonth>
{
    public int Year { get; }

    /// <summary>1 (January) to 12 (December).</summary>
    public int Month { get; }

    public BudgetMonth(int year, int month)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(year, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(year, 9999);
        ArgumentOutOfRangeException.ThrowIfLessThan(month, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(month, 12);
        Year = year;
        Month = month;
    }

    public bool Contains(DateOnly date) => date.Year == Year && date.Month == Month;

    public int CompareTo(BudgetMonth other) => (Year, Month).CompareTo((other.Year, other.Month));

    public override string ToString() => $"{Year:D4}-{Month:D2}";
}
