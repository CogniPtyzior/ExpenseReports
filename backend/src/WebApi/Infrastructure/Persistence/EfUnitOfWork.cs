// Implements the transaction boundary used by application services.
using WebApi.Application.Core.Persistence;

namespace WebApi.Infrastructure.Persistence;

/// <summary>
/// EF Core unit of work committing all pending changes for a use case.
/// </summary>
internal sealed class EfUnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}