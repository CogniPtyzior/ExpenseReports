// Defines supported currencies for expense entry amounts.
using WebApi.Core.Exceptions;

namespace WebApi.Domain.ExpenseEntries;

/// <summary>
/// Value object intentionally limited to EUR because the
/// current functional scope only requires euro expenses.
/// Currency conversions can be added when multi-currency
/// expense management becomes a supported use case.
/// </summary>
internal sealed class Currency
{
    public static readonly Currency Eur = new("EUR");

    private Currency()
    {
        Code = string.Empty;
    }

    private Currency(string code)
    {
        Code = code;
    }

    public string Code { get; private set; }

    public static Currency Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("expense_entry.currency_required", "Currency is required.");
        }

        var normalizedCode = code.Trim().ToUpperInvariant();
        if (normalizedCode != Eur.Code)
        {
            throw new DomainException("expense_entry.currency_unsupported", "Only EUR currency is supported.");
        }

        return new Currency(normalizedCode);
    }
}
