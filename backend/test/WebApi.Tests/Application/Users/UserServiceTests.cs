// Covers user application use cases with fake ports.
using FluentValidation;
using WebApi.Application.Core.Clock;
using WebApi.Application.Core.Observability;
using WebApi.Application.Core.Persistence;
using WebApi.Application.Users;
using WebApi.Core.Exceptions;
using WebApi.Domain.Users;

namespace WebApi.Tests.Application.Users;

public sealed class UserServiceTests
{
    private static readonly DateTime Now = new(2025, 10, 15, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Create_persists_active_user_and_commits()
    {
        var fixture = new Fixture();
        var service = fixture.CreateCommandService();

        var result = await service.CreateAsync(CreateCommand(), CancellationToken.None);

        Assert.Single(fixture.Repository.Users);
        Assert.True(result.IsActive);
        Assert.True(result.CanBeAssignedToExpenseReport);
        Assert.Equal("Juste Leblanc", result.FullName);
        Assert.Equal(1, fixture.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_missing_user_throws_not_found()
    {
        var service = new Fixture().CreateCommandService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(UpdateCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal("user.not_found", exception.Code);
    }

    [Fact]
    public async Task Deactivate_removes_user_assignability()
    {
        var fixture = new Fixture();
        var user = fixture.AddUser();
        var service = fixture.CreateCommandService();

        var result = await service.DeactivateAsync(user.Id, CancellationToken.None);

        Assert.False(result.IsActive);
        Assert.False(result.CanBeAssignedToExpenseReport);
        Assert.Equal(1, fixture.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Delete_soft_deletes_user_without_removing_repository_row()
    {
        var fixture = new Fixture();
        var user = fixture.AddUser();
        var service = fixture.CreateCommandService();

        await service.DeleteAsync(user.Id, CancellationToken.None);

        Assert.Single(fixture.Repository.Users);
        Assert.True(user.IsDeleted);
        Assert.Equal(1, fixture.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Assignable_query_excludes_inactive_and_deleted_users()
    {
        var fixture = new Fixture();
        fixture.AddUser("Active", "User");
        fixture.AddUser("Inactive", "User").Deactivate(Now);
        fixture.AddUser("Deleted", "User").SoftDelete(Now);
        var service = new UserQueryService(fixture.Repository);

        var users = await service.ListAssignableAsync(CancellationToken.None);

        var user = Assert.Single(users);
        Assert.Equal("Active User", user.FullName);
    }

    private static CreateUserCommand CreateCommand()
    {
        return new CreateUserCommand("Juste", "Leblanc", "1 Rue des Notes", "75001", "Paris", 5);
    }

    private static UpdateUserCommand UpdateCommand(Guid id)
    {
        return new UpdateUserCommand(id, "Marc", "Assin", "24 Avenue des Frais", "69002", "Lyon", 8);
    }

    private sealed class Fixture
    {
        public FakeUserRepository Repository { get; } = new();
        public FakeUnitOfWork UnitOfWork { get; } = new();

        public UserCommandService CreateCommandService()
        {
            return new UserCommandService(
                Repository,
                new CreateUserCommandValidator(),
                new UpdateUserCommandValidator(),
                new FixedClock(),
                UnitOfWork,
                new NullApplicationLogger());
        }

        public User AddUser(string firstName = "Juste", string lastName = "Leblanc")
        {
            var user = User.Create(
                Guid.NewGuid(),
                PersonName.Create(firstName, lastName),
                PostalAddress.Create("1 Rue des Notes", "75001", "Paris"),
                MonthlyExpenseQuota.Create(5),
                Now);
            Repository.Users.Add(user);
            return user;
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<User> Users { get; } = [];

        public Task<IReadOnlyCollection<User>> ListAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<User>>(Users.Where(user => !user.IsDeleted).ToArray());
        }

        public Task<IReadOnlyCollection<User>> ListAssignableAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<User>>(Users.Where(user => user.CanBeAssignedToExpenseReport).ToArray());
        }

        public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Users.FirstOrDefault(user => user.Id == id && !user.IsDeleted));
        }

        public Task AddAsync(User user, CancellationToken cancellationToken)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public async Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<CancellationToken, Task<TResult>> operation,
            CancellationToken cancellationToken)
        {
            return await operation(cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock : IClock
    {
        public DateTime UtcNow => Now;
    }

    private sealed class NullApplicationLogger : IApplicationLogger
    {
        public void Information(string message, params object[] args)
        {
        }
    }
}
