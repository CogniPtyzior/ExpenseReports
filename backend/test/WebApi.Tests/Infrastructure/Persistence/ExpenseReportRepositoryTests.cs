// Covers the EF Core expense report repository adapter.
using Microsoft.EntityFrameworkCore;
using WebApi.Domain.ExpenseReports;
using WebApi.Domain.Users;
using WebApi.Infrastructure.Persistence;

namespace WebApi.Tests.Infrastructure.Persistence;

public sealed class ExpenseReportRepositoryTests
{
    [Fact]
    public async Task Exists_for_user_and_month_detects_existing_report()
    {
        await using var db = CreateContext();
        var user = CreateUser();
        var report = CreateReport(user, 2025, 10);
        db.Add(user);
        db.Add(report);
        await db.SaveChangesAsync();
        var repository = new ExpenseReportRepository(db);

        var exists = await repository.ExistsForUserAndMonthAsync(user.Id, CalendarMonth.Create(2025, 10), CancellationToken.None);

        Assert.True(exists);
    }

    [Fact]
    public async Task List_async_orders_recent_reports_first()
    {
        await using var db = CreateContext();
        var user = CreateUser();
        db.Add(user);
        db.Add(CreateReport(user, 2025, 9));
        db.Add(CreateReport(user, 2025, 10));
        await db.SaveChangesAsync();
        var repository = new ExpenseReportRepository(db);

        var reports = await repository.ListAsync(CancellationToken.None);

        Assert.Collection(
            reports,
            report => Assert.Equal(10, report.Period.Month),
            report => Assert.Equal(9, report.Period.Month));
    }

    [Fact]
    public async Task Find_by_id_for_update_finds_existing_report()
    {
        await using var db = CreateContext();
        var user = CreateUser();
        var report = CreateReport(user, 2025, 10);
        db.Add(user);
        db.Add(report);
        await db.SaveChangesAsync();
        var repository = new ExpenseReportRepository(db);

        var found = await repository.FindByIdForUpdateAsync(report.Id, CancellationToken.None);

        Assert.NotNull(found);
        Assert.Equal(report.Id, found.Id);
    }
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static User CreateUser()
    {
        return User.Create(
            Guid.NewGuid(),
            PersonName.Create("Juste", "Leblanc"),
            PostalAddress.Create("1 Rue des Notes", "75001", "Paris"),
            MonthlyExpenseQuota.Create(5),
            DateTime.UtcNow);
    }

    private static ExpenseReport CreateReport(User user, int year, int month)
    {
        return ExpenseReport.Create(Guid.NewGuid(), user.Id, user.Name.FullName, CalendarMonth.Create(year, month), DateTime.UtcNow);
    }
}
