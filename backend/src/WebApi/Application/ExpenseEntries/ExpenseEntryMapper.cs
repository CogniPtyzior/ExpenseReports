// Maps expense entry aggregates to application read models.
using WebApi.Domain.ExpenseEntries;

namespace WebApi.Application.ExpenseEntries;

/// <summary>
/// Centralized mapper keeping endpoints independent from domain projection details.
/// </summary>
internal static class ExpenseEntryMapper
{
    public static ExpenseEntryResult ToResult(ExpenseEntry entry)
    {
        return new ExpenseEntryResult(
            entry.Id,
            entry.ExpenseReportId,
            entry.ReportYear,
            entry.ReportMonth,
            entry.ExpenseDate,
            entry.Description.Value,
            entry.Amount.Amount,
            entry.Amount.Currency.Code,
            entry.BillingAddress.MerchantName,
            entry.BillingAddress.Street,
            entry.BillingAddress.PostalCode,
            entry.BillingAddress.City,
            entry.CreatedAtUtc,
            entry.UpdatedAtUtc);
    }
}
