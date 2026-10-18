namespace BudgetLens.Domain.Budgeting;

internal static class Names
{
    /// <summary>Jar and item names are compared ignoring case ("Fun" and "fun" are the same jar).</summary>
    public static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    public static string Required(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A name is required.", paramName);
        }

        return name.Trim();
    }

    public static void EnsureUnique(IEnumerable<string> names, string what, string paramName)
    {
        var duplicate = names.GroupBy(name => name, Comparer).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new ArgumentException($"{what} names must be unique; '{duplicate.Key}' is repeated.", paramName);
        }
    }
}
