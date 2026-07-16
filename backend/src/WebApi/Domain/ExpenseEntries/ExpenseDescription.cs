// Defines the bounded description used by expense entries.
using WebApi.Core.Exceptions;

namespace WebApi.Domain.ExpenseEntries;

/// <summary>
/// Value object representing the required expense description.
/// </summary>
internal sealed class ExpenseDescription
{
    public const int MaxLength = 50;

    private ExpenseDescription()
    {
        Value = string.Empty;
    }

    private ExpenseDescription(string value)
    {
        Value = value;
    }

    public string Value { get; private set; }

    public static ExpenseDescription Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("expense_entry.description_required", "Description is required.");
        }

        var normalizedValue = value.Trim();
        if (normalizedValue.Length > MaxLength)
        {
            throw new DomainException("expense_entry.description_too_long", "Description cannot exceed 50 characters.");
        }

        return new ExpenseDescription(normalizedValue);
    }
}
