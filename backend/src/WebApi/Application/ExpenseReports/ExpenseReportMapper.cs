// Maps expense report aggregates to application read models.
using WebApi.Domain.ExpenseReports;

namespace WebApi.Application.ExpenseReports;

/// <summary>
/// Centralized mapper keeping endpoints independent from domain projection details.
/// </summary>
internal static class ExpenseReportMapper
{
    public static ExpenseReportResult ToResult(ExpenseReport report)
    {
        return new ExpenseReportResult(
            report.Id,
            report.UserId,
            report.AssignedUserFullName,
            report.Period.Year,
            report.Period.Month,
            report.Title,
            report.CreatedAtUtc);
    }
}