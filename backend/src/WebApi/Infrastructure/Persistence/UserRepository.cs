// Implements the user repository port with EF Core.
using Microsoft.EntityFrameworkCore;
using WebApi.Application.Users;
using WebApi.Domain.Users;

namespace WebApi.Infrastructure.Persistence;

/// <summary>
/// EF Core adapter for user persistence queries and commands.
/// </summary>
internal sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<IReadOnlyCollection<User>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Set<User>()
            .AsNoTracking()
            .Where(user => !user.IsDeleted)
            .OrderBy(user => user.Name.LastName)
            .ThenBy(user => user.Name.FirstName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<User>> ListAssignableAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Set<User>()
            .AsNoTracking()
            .Where(user => !user.IsDeleted && user.IsActive)
            .OrderBy(user => user.Name.LastName)
            .ThenBy(user => user.Name.FirstName)
            .ToArrayAsync(cancellationToken);
    }

    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Set<User>()
            .FirstOrDefaultAsync(user => user.Id == id && !user.IsDeleted, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await dbContext.Set<User>().AddAsync(user, cancellationToken);
    }
}