// Defines the persistence port used by expense report application services.
using WebApi.Domain.ExpenseReports;

namespace WebApi.Application.ExpenseReports;

/// <summary>
/// Port abstracting expense report persistence from application use cases.
/// </summary>
internal interface IExpenseReportRepository
{
    Task<IReadOnlyCollection<ExpenseReport>> ListAsync(CancellationToken cancellationToken);

    Task<ExpenseReport?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ExpenseReport?> FindByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsForUserAndMonthAsync(Guid userId, CalendarMonth period, CancellationToken cancellationToken);

    Task AddAsync(ExpenseReport report, CancellationToken cancellationToken);

    void Remove(ExpenseReport report);
}
