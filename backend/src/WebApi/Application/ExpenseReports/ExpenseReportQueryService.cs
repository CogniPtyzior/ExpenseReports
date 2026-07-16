// Implements expense report read use cases from repository ports.
using WebApi.Core.Exceptions;

namespace WebApi.Application.ExpenseReports;

/// <summary>
/// Coordinates expense report reads and returns presentation-safe read models.
/// </summary>
internal sealed class ExpenseReportQueryService(IExpenseReportRepository reports) : IExpenseReportQueryService
{
    public async Task<IReadOnlyCollection<ExpenseReportResult>> ListAsync(CancellationToken cancellationToken)
    {
        var result = await reports.ListAsync(cancellationToken);
        return result.Select(ExpenseReportMapper.ToResult).ToArray();
    }

    public async Task<ExpenseReportResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var report = await reports.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("expense_report.not_found", "Expense report was not found.");

        return ExpenseReportMapper.ToResult(report);
    }
}