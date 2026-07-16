// Implements the expense entry repository port with EF Core.
using Microsoft.EntityFrameworkCore;
using WebApi.Application.ExpenseEntries;
using WebApi.Domain.ExpenseEntries;

namespace WebApi.Infrastructure.Persistence;

/// <summary>
/// EF Core adapter for expense entry persistence queries and commands.
/// </summary>
internal sealed class ExpenseEntryRepository(AppDbContext dbContext) : IExpenseEntryRepository
{
    public async Task<IReadOnlyCollection<ExpenseEntry>> ListActiveByReportAsync(
        Guid expenseReportId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Set<ExpenseEntry>()
            .AsNoTracking()
            .Where(entry => entry.ExpenseReportId == expenseReportId && !entry.IsDeleted)
            .OrderBy(entry => entry.ExpenseDate)
            .ThenBy(entry => entry.CreatedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public Task<ExpenseEntry?> FindActiveByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Set<ExpenseEntry>()
            .FirstOrDefaultAsync(entry => entry.Id == id && !entry.IsDeleted, cancellationToken);
    }

    public async Task AddAsync(ExpenseEntry entry, CancellationToken cancellationToken)
    {
        await dbContext.Set<ExpenseEntry>().AddAsync(entry, cancellationToken);
    }
}
