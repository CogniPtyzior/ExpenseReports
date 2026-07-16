// Defines expense report read use cases exposed to the presentation layer.
namespace WebApi.Application.ExpenseReports;

/// <summary>
/// Application service contract for expense report queries.
/// </summary>
internal interface IExpenseReportQueryService
{
    Task<IReadOnlyCollection<ExpenseReportResult>> ListAsync(CancellationToken cancellationToken);

    Task<ExpenseReportResult> GetAsync(Guid id, CancellationToken cancellationToken);
}