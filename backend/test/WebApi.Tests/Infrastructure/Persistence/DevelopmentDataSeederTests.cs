// Covers the development seed data used by Aspire and manual smoke tests.
using Microsoft.EntityFrameworkCore;
using WebApi.Domain.ExpenseEntries;
using WebApi.Domain.ExpenseReports;
using WebApi.Domain.Users;
using WebApi.Infrastructure.Persistence;
using WebApi.Infrastructure.Persistence.Seeding;

namespace WebApi.Tests.Infrastructure.Persistence;

public sealed class DevelopmentDataSeederTests
{
    private static int CurrentYear => DateTime.UtcNow.Year;
    private static readonly Guid MarcAssinId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid MarcNovemberReportId = Guid.Parse("77777777-7777-7777-7777-777777777777");

    [Fact]
    public async Task Seed_async_adds_demo_report_with_six_active_entries_for_quota_eight_user()
    {
        await using var db = CreateContext();

        await DevelopmentDataSeeder.SeedAsync(db, CancellationToken.None);

        var marc = await db.Set<User>().SingleAsync(user => user.Id == MarcAssinId);
        var report = await db.Set<ExpenseReport>().SingleAsync(report => report.Id == MarcNovemberReportId);
        var entries = await db.Set<ExpenseEntry>()
            .Where(entry => entry.ExpenseReportId == report.Id && !entry.IsDeleted)
            .OrderBy(entry => entry.ExpenseDate)
            .ToArrayAsync();

        Assert.Equal(8, marc.MonthlyExpenseQuota.Value);
        Assert.Equal(MarcAssinId, report.UserId);
        Assert.Equal(CurrentYear, report.Year);
        Assert.Equal(11, report.Month);
        Assert.Equal(6, entries.Length);
        Assert.All(entries, entry => Assert.Equal(11, entry.ExpenseDate.Month));
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
