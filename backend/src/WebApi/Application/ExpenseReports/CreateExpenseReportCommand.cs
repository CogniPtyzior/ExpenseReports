// Defines the input contract used to create expense reports.
namespace WebApi.Application.ExpenseReports;

/// <summary>
/// Command carrying the assigned user and calendar period required to create an expense report.
/// </summary>
internal sealed record CreateExpenseReportCommand(Guid UserId, int Year, int Month);