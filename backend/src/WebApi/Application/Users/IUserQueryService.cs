// Defines user read use cases exposed to the presentation layer.
namespace WebApi.Application.Users;

/// <summary>
/// Application service contract for user queries.
/// </summary>
internal interface IUserQueryService
{
    Task<IReadOnlyCollection<UserResult>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<UserResult>> ListAssignableAsync(CancellationToken cancellationToken);

    Task<UserResult> GetAsync(Guid id, CancellationToken cancellationToken);
}