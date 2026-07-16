// Defines the EF Core database context and development seed data.
using Microsoft.EntityFrameworkCore;
using WebApi.Domain.ExpenseEntries;
using WebApi.Domain.ExpenseReports;
using WebApi.Domain.Users;

namespace WebApi.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the application persistence adapters.
/// </summary>
internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    private static readonly Guid JusteLeblancId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid MarcAssinId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid OctoberReportId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid OctoberLunchEntryId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid OctoberTaxiEntryId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid OctoberDeletedEntryId = Guid.Parse("66666666-6666-6666-6666-666666666666");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseReportEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseEntryEntityTypeConfiguration());
    }

    internal static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        var createdAtUtc = DateTime.UtcNow;
        var juste = await context.Set<User>().FirstOrDefaultAsync(user => user.Id == JusteLeblancId, cancellationToken);
        if (juste is null)
        {
            juste = User.Create(
                JusteLeblancId,
                PersonName.Create("Juste", "Leblanc"),
                PostalAddress.Create("1 Rue des Notes", "75001", "Paris"),
                MonthlyExpenseQuota.Create(5),
                createdAtUtc);
            context.Set<User>().Add(juste);
        }

        if (!await context.Set<User>().AnyAsync(user => user.Id == MarcAssinId, cancellationToken))
        {
            context.Set<User>().Add(User.Create(
                MarcAssinId,
                PersonName.Create("Marc", "Assin"),
                PostalAddress.Create("24 Avenue des Frais", "69002", "Lyon"),
                MonthlyExpenseQuota.Create(8),
                createdAtUtc));
        }

        if (!await context.Set<ExpenseReport>().AnyAsync(report => report.Id == OctoberReportId, cancellationToken))
        {
            context.Set<ExpenseReport>().Add(ExpenseReport.Create(
                OctoberReportId,
                juste.Id,
                juste.Name.FullName,
                CalendarMonth.Create(2025, 10),
                createdAtUtc));
        }

        await SeedExpenseEntriesAsync(context, createdAtUtc, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedExpenseEntriesAsync(
        DbContext context,
        DateTime createdAtUtc,
        CancellationToken cancellationToken)
    {
        var period = CalendarMonth.Create(2025, 10);
        if (!await context.Set<ExpenseEntry>().AnyAsync(entry => entry.Id == OctoberLunchEntryId, cancellationToken))
        {
            context.Set<ExpenseEntry>().Add(CreateEntry(
                OctoberLunchEntryId,
                period,
                new DateOnly(2025, 10, 15),
                "Lunch",
                25m,
                createdAtUtc));
        }

        if (!await context.Set<ExpenseEntry>().AnyAsync(entry => entry.Id == OctoberTaxiEntryId, cancellationToken))
        {
            context.Set<ExpenseEntry>().Add(CreateEntry(
                OctoberTaxiEntryId,
                period,
                new DateOnly(2025, 10, 20),
                "Taxi",
                42m,
                createdAtUtc));
        }

        if (!await context.Set<ExpenseEntry>().AnyAsync(entry => entry.Id == OctoberDeletedEntryId, cancellationToken))
        {
            var deletedEntry = CreateEntry(
                OctoberDeletedEntryId,
                period,
                new DateOnly(2025, 10, 25),
                "Cancelled meal",
                18m,
                createdAtUtc);
            deletedEntry.SoftDelete(createdAtUtc);
            context.Set<ExpenseEntry>().Add(deletedEntry);
        }
    }

    private static ExpenseEntry CreateEntry(
        Guid id,
        CalendarMonth period,
        DateOnly expenseDate,
        string description,
        decimal amount,
        DateTime createdAtUtc)
    {
        return ExpenseEntry.Create(
            id,
            OctoberReportId,
            period,
            expenseDate,
            ExpenseDescription.Create(description),
            Money.Create(amount, Currency.Eur),
            BillingAddress.Create("Cafe Paris", "1 Rue des Notes", "75001", "Paris"),
            createdAtUtc);
    }
}
