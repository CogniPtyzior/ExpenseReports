// Covers the EF Core user repository adapter.
using Microsoft.EntityFrameworkCore;
using WebApi.Domain.Users;
using WebApi.Infrastructure.Persistence;

namespace WebApi.Tests.Infrastructure.Persistence;

public sealed class UserRepositoryTests
{
    [Fact]
    public async Task List_async_excludes_soft_deleted_users()
    {
        await using var db = CreateContext();
        var active = CreateUser("Active", "User");
        var deleted = CreateUser("Deleted", "User");
        deleted.SoftDelete(DateTime.UtcNow);
        db.AddRange(active, deleted);
        await db.SaveChangesAsync();
        var repository = new UserRepository(db);

        var users = await repository.ListAsync(CancellationToken.None);

        var user = Assert.Single(users);
        Assert.Equal("Active", user.Name.FirstName);
    }

    [Fact]
    public async Task List_assignable_async_excludes_inactive_users()
    {
        await using var db = CreateContext();
        var active = CreateUser("Active", "User");
        var inactive = CreateUser("Inactive", "User");
        inactive.Deactivate(DateTime.UtcNow);
        db.AddRange(active, inactive);
        await db.SaveChangesAsync();
        var repository = new UserRepository(db);

        var users = await repository.ListAssignableAsync(CancellationToken.None);

        var user = Assert.Single(users);
        Assert.Equal("Active", user.Name.FirstName);
    }

    [Fact]
    public async Task Find_by_id_returns_null_for_soft_deleted_user()
    {
        await using var db = CreateContext();
        var deleted = CreateUser("Deleted", "User");
        deleted.SoftDelete(DateTime.UtcNow);
        db.Add(deleted);
        await db.SaveChangesAsync();
        var repository = new UserRepository(db);

        var user = await repository.FindByIdAsync(deleted.Id, CancellationToken.None);

        Assert.Null(user);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static User CreateUser(string firstName, string lastName)
    {
        return User.Create(
            Guid.NewGuid(),
            PersonName.Create(firstName, lastName),
            PostalAddress.Create("1 Rue des Notes", "75001", "Paris"),
            MonthlyExpenseQuota.Create(5),
            DateTime.UtcNow);
    }
}