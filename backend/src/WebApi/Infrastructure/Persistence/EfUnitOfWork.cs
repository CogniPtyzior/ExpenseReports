// Implements the transaction boundary used by application services.
using Microsoft.EntityFrameworkCore;
using WebApi.Application.Core.Persistence;

namespace WebApi.Infrastructure.Persistence;

/// <summary>
/// EF Core unit of work committing all pending changes for a use case.
/// </summary>
internal sealed class EfUnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        });
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
