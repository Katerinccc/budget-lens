using BudgetLens.Domain.Common;

namespace BudgetLens.Domain.Budgeting;

/// <summary>One planned amount inside a jar, e.g. "Rent" in Fixed expenses.</summary>
public sealed record PlannedItem
{
    public string Name { get; }

    public Money Amount { get; }

    public PlannedItem(string name, Money amount)
    {
        if (amount.Amount <= 0m)
        {
            throw new ArgumentException("A planned amount must be greater than zero.", nameof(amount));
        }

        Name = Names.Required(name, nameof(name));
        Amount = amount;
    }
}
