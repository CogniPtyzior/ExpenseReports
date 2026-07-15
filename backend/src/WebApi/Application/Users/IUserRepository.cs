// Defines the persistence port used by user application services.
using WebApi.Domain.Users;

namespace WebApi.Application.Users;

/// <summary>
/// Port abstracting user persistence so use cases remain independent from EF Core.
/// </summary>
internal interface IUserRepository
{
    Task<IReadOnlyCollection<User>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<User>> ListAssignableAsync(CancellationToken cancellationToken);

    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);
}