// Defines expense report write use cases exposed to the presentation layer.
namespace WebApi.Application.ExpenseReports;

/// <summary>
/// Application service contract for expense report mutations.
/// </summary>
internal interface IExpenseReportCommandService
{
    Task<ExpenseReportResult> CreateAsync(CreateExpenseReportCommand command, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
