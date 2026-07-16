// Defines the input contract for updating an expense entry.
namespace WebApi.Application.ExpenseEntries;

/// <summary>
/// Command carrying the data required to update an existing expense entry.
/// </summary>
internal sealed record UpdateExpenseEntryCommand(
    Guid Id,
    DateOnly ExpenseDate,
    string Description,
    decimal Amount,
    string MerchantName,
    string Street,
    string PostalCode,
    string City);
