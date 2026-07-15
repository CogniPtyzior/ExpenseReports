// Implements user read use cases from repository ports.
using WebApi.Core.Exceptions;

namespace WebApi.Application.Users;

/// <summary>
/// Coordinates user reads and returns presentation-safe read models.
/// </summary>
internal sealed class UserQueryService(IUserRepository users) : IUserQueryService
{
    public async Task<IReadOnlyCollection<UserResult>> ListAsync(CancellationToken cancellationToken)
    {
        var result = await users.ListAsync(cancellationToken);
        return result.Select(UserMapper.ToResult).ToArray();
    }

    public async Task<IReadOnlyCollection<UserResult>> ListAssignableAsync(CancellationToken cancellationToken)
    {
        var result = await users.ListAssignableAsync(cancellationToken);
        return result.Select(UserMapper.ToResult).ToArray();
    }

    public async Task<UserResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("user.not_found", "User was not found.");

        return UserMapper.ToResult(user);
    }
}