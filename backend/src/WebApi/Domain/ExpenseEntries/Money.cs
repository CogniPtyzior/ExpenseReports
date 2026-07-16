// Defines monetary amounts used by expense entries.
using WebApi.Core.Exceptions;

namespace WebApi.Domain.ExpenseEntries;

/// <summary>
/// Value object representing a strictly positive EUR amount
/// without unnecessary financial operations.
/// </summary>
internal sealed class Money
{
    private Money()
    {
        Currency = null!;
    }

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; private set; }
    public Currency Currency { get; private set; }

    public static Money Create(decimal amount, Currency currency)
    {
        if (currency is null)
        {
            throw new DomainException("expense_entry.currency_required", "Currency is required.");
        }

        if (amount <= 0)
        {
            throw new DomainException("expense_entry.amount_invalid", "Amount must be strictly positive.");
        }

        return new Money(amount, currency);
    }
}
