// Defines user write use cases exposed to the presentation layer.
namespace WebApi.Application.Users;

/// <summary>
/// Application service contract for user mutations.
/// </summary>
internal interface IUserCommandService
{
    Task<UserResult> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken);

    Task<UserResult> UpdateAsync(UpdateUserCommand command, CancellationToken cancellationToken);

    Task<UserResult> ActivateAsync(Guid id, CancellationToken cancellationToken);

    Task<UserResult> DeactivateAsync(Guid id, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}