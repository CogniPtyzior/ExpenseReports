// Defines the expense entry read model returned by application use cases.
namespace WebApi.Application.ExpenseEntries;

/// <summary>
/// Read model exposing an expense entry without leaking persistence concerns.
/// </summary>
internal sealed record ExpenseEntryResult(
    Guid Id,
    Guid ExpenseReportId,
    int ReportYear,
    int ReportMonth,
    DateOnly ExpenseDate,
    string Description,
    decimal Amount,
    string Currency,
    string MerchantName,
    string Street,
    string PostalCode,
    string City,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
