// Implements the expense report repository port with EF Core.
using Microsoft.EntityFrameworkCore;
using WebApi.Application.ExpenseReports;
using WebApi.Domain.ExpenseReports;

namespace WebApi.Infrastructure.Persistence;

/// <summary>
/// EF Core adapter for expense report persistence queries and commands.
/// </summary>
internal sealed class ExpenseReportRepository(AppDbContext dbContext) : IExpenseReportRepository
{
    public async Task<IReadOnlyCollection<ExpenseReport>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Set<ExpenseReport>()
            .AsNoTracking()
            .OrderByDescending(report => report.Year)
            .ThenByDescending(report => report.Month)
            .ThenBy(report => report.AssignedUserFullName)
            .ToArrayAsync(cancellationToken);
    }

    public Task<ExpenseReport?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Set<ExpenseReport>()
            .AsNoTracking()
            .FirstOrDefaultAsync(report => report.Id == id, cancellationToken);
    }

    public Task<ExpenseReport?> FindByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        if (dbContext.Database.ProviderName is not "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            return FindByIdAsync(id, cancellationToken);
        }

        return dbContext.Set<ExpenseReport>()
            .FromSqlInterpolated($@"SELECT * FROM expense_reports WHERE ""Id"" = {id} FOR UPDATE")
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> ExistsForUserAndMonthAsync(Guid userId, CalendarMonth period, CancellationToken cancellationToken)
    {
        return dbContext.Set<ExpenseReport>()
            .AnyAsync(report => report.UserId == userId
                && report.Year == period.Year
                && report.Month == period.Month, cancellationToken);
    }

    public async Task AddAsync(ExpenseReport report, CancellationToken cancellationToken)
    {
        await dbContext.Set<ExpenseReport>().AddAsync(report, cancellationToken);
    }

    public void Remove(ExpenseReport report)
    {
        dbContext.Set<ExpenseReport>().Remove(report);
    }
}