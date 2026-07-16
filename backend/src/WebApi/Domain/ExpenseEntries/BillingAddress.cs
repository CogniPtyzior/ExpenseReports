// Defines billing address data captured for each expense entry.
using WebApi.Core.Exceptions;

namespace WebApi.Domain.ExpenseEntries;

/// <summary>
/// Value object representing the invoice address attached to an expense.
/// </summary>
internal sealed class BillingAddress
{
    private BillingAddress()
    {
        MerchantName = string.Empty;
        Street = string.Empty;
        PostalCode = string.Empty;
        City = string.Empty;
    }

    private BillingAddress(string merchantName, string street, string postalCode, string city)
    {
        MerchantName = merchantName;
        Street = street;
        PostalCode = postalCode;
        City = city;
    }

    public string MerchantName { get; private set; }
    public string Street { get; private set; }
    public string PostalCode { get; private set; }
    public string City { get; private set; }

    public static BillingAddress Create(string merchantName, string street, string postalCode, string city)
    {
        return new BillingAddress(
            Required(merchantName, "expense_entry.billing_merchant_required", "Billing merchant name is required."),
            Required(street, "expense_entry.billing_street_required", "Billing street is required."),
            Required(postalCode, "expense_entry.billing_postal_code_required", "Billing postal code is required."),
            Required(city, "expense_entry.billing_city_required", "Billing city is required."));
    }

    private static string Required(string value, string code, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(code, message);
        }

        return value.Trim();
    }
}
