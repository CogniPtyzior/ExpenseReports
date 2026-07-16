// Defines the input contract for creating an expense entry.
namespace WebApi.Application.ExpenseEntries;

/// <summary>
/// Command carrying the data required to attach a new expense entry to an expense report.
/// </summary>
internal sealed record CreateExpenseEntryCommand(
    Guid ExpenseReportId,
    DateOnly ExpenseDate,
    string Description,
    decimal Amount,
    string MerchantName,
    string Street,
    string PostalCode,
    string City);
