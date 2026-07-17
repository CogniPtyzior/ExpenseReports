// Defines expense entry read use cases.
using WebApi.Core.Pagination;

namespace WebApi.Application.ExpenseEntries;

/// <summary>
/// Query port exposing active expense entries without leaking persistence details.
/// </summary>
internal interface IExpenseEntryQueryService
{
    Task<PagedResult<ExpenseEntryResult>> ListByReportAsync(
        Guid expenseReportId,
        int pageNumber,
        CancellationToken cancellationToken);
}