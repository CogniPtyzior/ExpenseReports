// Implements user write use cases and coordinates domain, persistence, and transactions.
using FluentValidation;
using WebApi.Application.Core.Clock;
using WebApi.Application.Core.Observability;
using WebApi.Application.Core.Persistence;
using WebApi.Core.Exceptions;
using WebApi.Domain.Users;

namespace WebApi.Application.Users;

/// <summary>
/// Coordinates user mutations while keeping domain rules in domain objects.
/// </summary>
internal sealed class UserCommandService(
    IUserRepository users,
    IValidator<CreateUserCommand> createValidator,
    IValidator<UpdateUserCommand> updateValidator,
    IClock clock,
    IUnitOfWork unitOfWork,
    IApplicationLogger logger) : IUserCommandService
{
    public async Task<UserResult> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(command, cancellationToken);

        var user = User.Create(
            Guid.NewGuid(),
            PersonName.Create(command.FirstName, command.LastName),
            PostalAddress.Create(command.Street, command.PostalCode, command.City),
            MonthlyExpenseQuota.Create(command.MonthlyExpenseQuota),
            clock.UtcNow);

        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.Information("User {UserId} was created.", user.Id);

        return UserMapper.ToResult(user);
    }

    public async Task<UserResult> UpdateAsync(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(command, cancellationToken);

        var user = await GetExistingUserAsync(command.Id, cancellationToken);
        user.Update(
            PersonName.Create(command.FirstName, command.LastName),
            PostalAddress.Create(command.Street, command.PostalCode, command.City),
            MonthlyExpenseQuota.Create(command.MonthlyExpenseQuota),
            clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.Information("User {UserId} was updated.", user.Id);

        return UserMapper.ToResult(user);
    }

    public async Task<UserResult> ActivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await GetExistingUserAsync(id, cancellationToken);
        user.Activate(clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.Information("User {UserId} was activated.", user.Id);

        return UserMapper.ToResult(user);
    }

    public async Task<UserResult> DeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await GetExistingUserAsync(id, cancellationToken);
        user.Deactivate(clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.Information("User {UserId} was deactivated.", user.Id);

        return UserMapper.ToResult(user);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await GetExistingUserAsync(id, cancellationToken);
        user.SoftDelete(clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.Information("User {UserId} was soft-deleted.", user.Id);
    }

    private async Task<User> GetExistingUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(id, cancellationToken);
        return user ?? throw new NotFoundException("user.not_found", "User was not found.");
    }
}