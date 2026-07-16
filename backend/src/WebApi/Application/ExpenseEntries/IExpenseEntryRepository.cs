// Defines the persistence port used by expense entry application services.
using WebApi.Domain.ExpenseEntries;

namespace WebApi.Application.ExpenseEntries;

/// <summary>
/// Port abstracting expense entry persistence from application use cases.
/// </summary>
internal interface IExpenseEntryRepository
{
    Task<IReadOnlyCollection<ExpenseEntry>> ListActiveByReportAsync(Guid expenseReportId, CancellationToken cancellationToken);

    Task<ExpenseEntry?> FindActiveByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(ExpenseEntry entry, CancellationToken cancellationToken);
}
