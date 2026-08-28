// Covers the EF Core expense entry repository adapter.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using WebApi.Domain.ExpenseEntries;
using WebApi.Domain.ExpenseReports;
using WebApi.Domain.Users;
using WebApi.Infrastructure.Persistence;

namespace WebApi.Tests.Infrastructure.Persistence;

public sealed class ExpenseEntryRepositoryTests
{
    private static int CurrentYear => DateTime.UtcNow.Year;
    [Fact]
    public async Task List_active_by_report_excludes_soft_deleted_entries_and_other_reports()
    {
        using var db = CreateContext();
        var user = CreateUser();
        var report = CreateReport(user);
        var otherReport = CreateReport(user, CurrentYear, 11);
        var activeEntry = CreateEntry(report, new DateOnly(CurrentYear, 10, 15), "Lunch");
        var deletedEntry = CreateEntry(report, new DateOnly(CurrentYear, 10, 16), "Taxi");
        deletedEntry.SoftDelete(DateTime.UtcNow);
        db.AddRange(
            user,
            report,
            otherReport,
            activeEntry,
            deletedEntry,
            CreateEntry(otherReport, new DateOnly(CurrentYear, 11, 1), "Hotel"));
        await db.SaveChangesAsync();
        var repository = new ExpenseEntryRepository(db);

        var entries = await repository.ListActiveByReportAsync(report.Id, CancellationToken.None);

        var entry = Assert.Single(entries);
        Assert.Equal(activeEntry.Id, entry.Id);
    }

    [Fact]
    public async Task Count_active_by_report_excludes_soft_deleted_entries_and_other_reports()
    {
        using var db = CreateContext();
        var user = CreateUser();
        var report = CreateReport(user);
        var otherReport = CreateReport(user, CurrentYear, 11);
        var deletedEntry = CreateEntry(report, new DateOnly(CurrentYear, 10, 16), "Taxi");
        deletedEntry.SoftDelete(DateTime.UtcNow);
        db.AddRange(
            user,
            report,
            otherReport,
            CreateEntry(report, new DateOnly(CurrentYear, 10, 15), "Lunch"),
            deletedEntry,
            CreateEntry(otherReport, new DateOnly(CurrentYear, 11, 1), "Hotel"));
        await db.SaveChangesAsync();
        var repository = new ExpenseEntryRepository(db);

        var count = await repository.CountActiveByReportAsync(report.Id, CancellationToken.None);

        Assert.Equal(1, count);
    }
    [Fact]
    public async Task List_active_by_report_applies_pagination_after_filtering_deleted_entries()
    {
        using var db = CreateContext();
        var user = CreateUser();
        var report = CreateReport(user);
        var deletedEntry = CreateEntry(report, new DateOnly(CurrentYear, 10, 11), "Deleted");
        deletedEntry.SoftDelete(DateTime.UtcNow);
        db.AddRange(
            user,
            report,
            CreateEntry(report, new DateOnly(CurrentYear, 10, 10), "A"),
            deletedEntry,
            CreateEntry(report, new DateOnly(CurrentYear, 10, 12), "B"),
            CreateEntry(report, new DateOnly(CurrentYear, 10, 13), "C"));
        await db.SaveChangesAsync();
        var repository = new ExpenseEntryRepository(db);

        var entries = await repository.ListActiveByReportAsync(report.Id, 2, 2, CancellationToken.None);

        var entry = Assert.Single(entries);
        Assert.Equal("C", entry.Description.Value);
    }
    [Fact]
    public async Task List_active_by_report_orders_entries_by_expense_date()
    {
        using var db = CreateContext();
        var user = CreateUser();
        var report = CreateReport(user);
        db.AddRange(
            user,
            report,
            CreateEntry(report, new DateOnly(CurrentYear, 10, 20), "Taxi"),
            CreateEntry(report, new DateOnly(CurrentYear, 10, 10), "Lunch"));
        await db.SaveChangesAsync();
        var repository = new ExpenseEntryRepository(db);

        var entries = await repository.ListActiveByReportAsync(report.Id, CancellationToken.None);

        Assert.Collection(
            entries,
            entry => Assert.Equal(new DateOnly(CurrentYear, 10, 10), entry.ExpenseDate),
            entry => Assert.Equal(new DateOnly(CurrentYear, 10, 20), entry.ExpenseDate));
    }

    [Fact]
    public async Task Find_active_by_id_returns_only_non_deleted_entry()
    {
        using var db = CreateContext();
        var user = CreateUser();
        var report = CreateReport(user);
        var entry = CreateEntry(report, new DateOnly(CurrentYear, 10, 15), "Lunch");
        var deletedEntry = CreateEntry(report, new DateOnly(CurrentYear, 10, 16), "Taxi");
        deletedEntry.SoftDelete(DateTime.UtcNow);
        db.AddRange(user, report, entry, deletedEntry);
        await db.SaveChangesAsync();
        var repository = new ExpenseEntryRepository(db);

        var found = await repository.FindActiveByIdAsync(entry.Id, CancellationToken.None);
        var deleted = await repository.FindActiveByIdAsync(deletedEntry.Id, CancellationToken.None);

        Assert.NotNull(found);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task Add_async_persists_value_objects()
    {
        using var db = CreateContext();
        var user = CreateUser();
        var report = CreateReport(user);
        db.AddRange(user, report);
        var repository = new ExpenseEntryRepository(db);
        var entry = CreateEntry(report, new DateOnly(CurrentYear, 10, 15), "Lunch");

        await repository.AddAsync(entry, CancellationToken.None);
        await db.SaveChangesAsync();

        var saved = await db.Set<ExpenseEntry>().SingleAsync();
        Assert.Equal("Lunch", saved.Description.Value);
        Assert.Equal(25m, saved.Amount.Amount);
        Assert.Equal("EUR", saved.Amount.Currency.Code);
        Assert.Equal("Cafe Paris", saved.BillingAddress.MerchantName);
    }

    [Fact]
    public void Expense_entry_mapping_protects_persistence_invariants()
    {
        using var db = CreateRelationalMetadataContext();
        var entity = db.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(ExpenseEntry));

        Assert.NotNull(entity);
        Assert.Equal("date", entity.FindProperty(nameof(ExpenseEntry.ExpenseDate))?.GetColumnType());
        Assert.Contains(
            entity.GetCheckConstraints(),
            constraint => constraint.Name == "CK_expense_entries_amount_positive");
        Assert.Contains(
            entity.GetCheckConstraints(),
            constraint => constraint.Name == "CK_expense_entries_currency_eur");
        Assert.Contains(
            entity.GetCheckConstraints(),
            constraint => constraint.Name == "CK_expense_entries_report_month_valid");
        Assert.Contains(
            entity.GetCheckConstraints(),
            constraint => constraint.Name == "CK_expense_entries_expense_date_report_month");
        Assert.Contains(entity.GetIndexes(), index => index.Properties.Select(property => property.Name).SequenceEqual([
            nameof(ExpenseEntry.ExpenseReportId),
            nameof(ExpenseEntry.IsDeleted),
            nameof(ExpenseEntry.ExpenseDate)
        ]));
        Assert.Contains(entity.GetForeignKeys(), foreignKey =>
            foreignKey.DeleteBehavior == DeleteBehavior.Cascade
            && foreignKey.Properties.Select(property => property.Name).SequenceEqual([
                nameof(ExpenseEntry.ExpenseReportId),
                nameof(ExpenseEntry.ReportYear),
                nameof(ExpenseEntry.ReportMonth)
            ]));
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static AppDbContext CreateRelationalMetadataContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=metadata;Username=metadata;Password=metadata")
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

    private static ExpenseReport CreateReport(User user, int? year = null, int month = 10)
    {
        return ExpenseReport.Create(Guid.NewGuid(), user.Id, user.Name.FullName, CalendarMonth.Create(year ?? CurrentYear, month), DateTime.UtcNow);
    }

    private static ExpenseEntry CreateEntry(ExpenseReport report, DateOnly expenseDate, string description)
    {
        return ExpenseEntry.Create(
            Guid.NewGuid(),
            report.Id,
            report.Period,
            expenseDate,
            ExpenseDescription.Create(description),
            Money.Create(25m, Currency.Eur),
            BillingAddress.Create("Cafe Paris", "1 Rue des Notes", "75001", "Paris"),
            DateTime.UtcNow);
    }
}
