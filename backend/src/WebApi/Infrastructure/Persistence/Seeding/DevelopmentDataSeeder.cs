// Provides idempotent development data for Aspire and manual smoke tests.
using Microsoft.EntityFrameworkCore;
using WebApi.Domain.ExpenseEntries;
using WebApi.Domain.ExpenseReports;
using WebApi.Domain.Users;

namespace WebApi.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds deterministic development data without mixing demo records into the DbContext definition.
/// </summary>
internal static class DevelopmentDataSeeder
{
    private static readonly Guid JusteLeblancId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid MarcAssinId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid OctoberReportId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid MarcNovemberReportId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    private static readonly Guid OctoberLunchEntryId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid OctoberTaxiEntryId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid OctoberDeletedEntryId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    private static readonly Guid MarcNovemberTrainEntryId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    private static readonly Guid MarcNovemberHotelEntryId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    private static readonly Guid MarcNovemberLunchEntryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid MarcNovemberTaxiEntryId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid MarcNovemberParkingEntryId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid MarcNovemberDinnerEntryId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    internal static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        var createdAtUtc = DateTime.UtcNow;
        var juste = await GetOrCreateJusteAsync(context, createdAtUtc, cancellationToken);
        var marc = await GetOrCreateMarcAsync(context, createdAtUtc, cancellationToken);

        await SeedExpenseReportsAsync(context, juste, marc, createdAtUtc, cancellationToken);
        await SeedExpenseEntriesAsync(context, createdAtUtc, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task<User> GetOrCreateJusteAsync(
        DbContext context,
        DateTime createdAtUtc,
        CancellationToken cancellationToken)
    {
        var juste = await context.Set<User>().FirstOrDefaultAsync(user => user.Id == JusteLeblancId, cancellationToken);
        if (juste is not null)
        {
            return juste;
        }

        juste = User.Create(
            JusteLeblancId,
            PersonName.Create("Juste", "Leblanc"),
            PostalAddress.Create("1 Rue des Notes", "75001", "Paris"),
            MonthlyExpenseQuota.Create(5),
            createdAtUtc);
        context.Set<User>().Add(juste);
        return juste;
    }

    private static async Task<User> GetOrCreateMarcAsync(
        DbContext context,
        DateTime createdAtUtc,
        CancellationToken cancellationToken)
    {
        var marc = await context.Set<User>().FirstOrDefaultAsync(user => user.Id == MarcAssinId, cancellationToken);
        if (marc is not null)
        {
            return marc;
        }

        marc = User.Create(
            MarcAssinId,
            PersonName.Create("Marc", "Assin"),
            PostalAddress.Create("24 Avenue des Frais", "69002", "Lyon"),
            MonthlyExpenseQuota.Create(8),
            createdAtUtc);
        context.Set<User>().Add(marc);
        return marc;
    }

    private static async Task SeedExpenseReportsAsync(
        DbContext context,
        User juste,
        User marc,
        DateTime createdAtUtc,
        CancellationToken cancellationToken)
    {
        if (!await context.Set<ExpenseReport>().AnyAsync(report => report.Id == OctoberReportId, cancellationToken))
        {
            context.Set<ExpenseReport>().Add(ExpenseReport.Create(
                OctoberReportId,
                juste.Id,
                juste.Name.FullName,
                CalendarMonth.Create(2025, 10),
                createdAtUtc));
        }

        if (!await context.Set<ExpenseReport>().AnyAsync(report => report.Id == MarcNovemberReportId, cancellationToken))
        {
            context.Set<ExpenseReport>().Add(ExpenseReport.Create(
                MarcNovemberReportId,
                marc.Id,
                marc.Name.FullName,
                CalendarMonth.Create(2025, 11),
                createdAtUtc));
        }
    }

    private static async Task SeedExpenseEntriesAsync(
        DbContext context,
        DateTime createdAtUtc,
        CancellationToken cancellationToken)
    {
        await SeedJusteOctoberEntriesAsync(context, createdAtUtc, cancellationToken);
        await SeedMarcNovemberEntriesAsync(context, createdAtUtc, cancellationToken);
    }

    private static async Task SeedJusteOctoberEntriesAsync(
        DbContext context,
        DateTime createdAtUtc,
        CancellationToken cancellationToken)
    {
        var period = CalendarMonth.Create(2025, 10);
        if (!await context.Set<ExpenseEntry>().AnyAsync(entry => entry.Id == OctoberLunchEntryId, cancellationToken))
        {
            context.Set<ExpenseEntry>().Add(CreateEntry(
                OctoberLunchEntryId,
                OctoberReportId,
                period,
                new DateOnly(2025, 10, 15),
                "Lunch",
                25m,
                "Cafe Paris",
                "1 Rue des Notes",
                "75001",
                "Paris",
                createdAtUtc));
        }

        if (!await context.Set<ExpenseEntry>().AnyAsync(entry => entry.Id == OctoberTaxiEntryId, cancellationToken))
        {
            context.Set<ExpenseEntry>().Add(CreateEntry(
                OctoberTaxiEntryId,
                OctoberReportId,
                period,
                new DateOnly(2025, 10, 20),
                "Taxi",
                42m,
                "Taxi Parisien",
                "12 Rue de Rivoli",
                "75004",
                "Paris",
                createdAtUtc));
        }

        if (!await context.Set<ExpenseEntry>().AnyAsync(entry => entry.Id == OctoberDeletedEntryId, cancellationToken))
        {
            var deletedEntry = CreateEntry(
                OctoberDeletedEntryId,
                OctoberReportId,
                period,
                new DateOnly(2025, 10, 25),
                "Cancelled meal",
                18m,
                "Cafe Paris",
                "1 Rue des Notes",
                "75001",
                "Paris",
                createdAtUtc);
            deletedEntry.SoftDelete(createdAtUtc);
            context.Set<ExpenseEntry>().Add(deletedEntry);
        }
    }

    private static async Task SeedMarcNovemberEntriesAsync(
        DbContext context,
        DateTime createdAtUtc,
        CancellationToken cancellationToken)
    {
        var period = CalendarMonth.Create(2025, 11);
        var entries = new[]
        {
            (MarcNovemberTrainEntryId, new DateOnly(2025, 11, 3), "Train", 86m, "SNCF Connect"),
            (MarcNovemberHotelEntryId, new DateOnly(2025, 11, 4), "Hotel", 132m, "Hotel Lumiere"),
            (MarcNovemberLunchEntryId, new DateOnly(2025, 11, 5), "Lunch", 28m, "Bistrot Central"),
            (MarcNovemberTaxiEntryId, new DateOnly(2025, 11, 6), "Taxi", 34m, "Taxi Lyon"),
            (MarcNovemberParkingEntryId, new DateOnly(2025, 11, 7), "Parking", 19m, "Parking Bellecour"),
            (MarcNovemberDinnerEntryId, new DateOnly(2025, 11, 8), "Dinner", 48m, "Brasserie Nord")
        };

        foreach (var (id, expenseDate, description, amount, merchantName) in entries)
        {
            if (await context.Set<ExpenseEntry>().AnyAsync(entry => entry.Id == id, cancellationToken))
            {
                continue;
            }

            context.Set<ExpenseEntry>().Add(CreateEntry(
                id,
                MarcNovemberReportId,
                period,
                expenseDate,
                description,
                amount,
                merchantName,
                "24 Avenue des Frais",
                "69002",
                "Lyon",
                createdAtUtc));
        }
    }

    private static ExpenseEntry CreateEntry(
        Guid id,
        Guid expenseReportId,
        CalendarMonth period,
        DateOnly expenseDate,
        string description,
        decimal amount,
        string merchantName,
        string street,
        string postalCode,
        string city,
        DateTime createdAtUtc)
    {
        return ExpenseEntry.Create(
            id,
            expenseReportId,
            period,
            expenseDate,
            ExpenseDescription.Create(description),
            Money.Create(amount, Currency.Eur),
            BillingAddress.Create(merchantName, street, postalCode, city),
            createdAtUtc);
    }
}
