// Implements expense entry read use cases with pagination rules.
using Microsoft.Extensions.Options;
using WebApi.Application.Core.Configuration;
using WebApi.Application.ExpenseReports;
using WebApi.Core.Exceptions;
using WebApi.Core.Pagination;

namespace WebApi.Application.ExpenseEntries;

/// <summary>
/// Reads active expense entries for one report while enforcing the configured page size.
/// </summary>
internal sealed class ExpenseEntryQueryService(
    IExpenseEntryRepository entries,
    IExpenseReportRepository reports,
    IOptions<ExpenseRulesOptions> options) : IExpenseEntryQueryService
{
    public async Task<PagedResult<ExpenseEntryResult>> ListByReportAsync(
        Guid expenseReportId,
        int pageNumber,
        CancellationToken cancellationToken)
    {
        if (expenseReportId == Guid.Empty)
        {
            throw new NotFoundException("expense_report.not_found", "Expense report was not found.");
        }

        if (pageNumber < 1)
        {
            throw new FluentValidation.ValidationException("Page number must be greater than or equal to 1.");
        }

        var report = await reports.FindByIdAsync(expenseReportId, cancellationToken)
            ?? throw new NotFoundException("expense_report.not_found", "Expense report was not found.");
        var pageSize = options.Value.ExpenseEntriesPageSize;
        var totalCount = await entries.CountActiveByReportAsync(report.Id, cancellationToken);
        var page = await entries.ListActiveByReportAsync(report.Id, pageNumber, pageSize, cancellationToken);

        return new PagedResult<ExpenseEntryResult>(
            page.Select(ExpenseEntryMapper.ToResult).ToArray(),
            pageNumber,
            pageSize,
            totalCount);
    }
}