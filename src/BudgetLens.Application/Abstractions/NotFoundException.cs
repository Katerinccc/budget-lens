namespace BudgetLens.Application.Abstractions;

/// <summary>The requested data does not exist. Driving adapters map it to NOT_FOUND / 404.</summary>
public sealed class NotFoundException(string message) : Exception(message);
