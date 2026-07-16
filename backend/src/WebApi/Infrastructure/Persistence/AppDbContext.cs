// Defines the EF Core database context and development seed data.
using Microsoft.EntityFrameworkCore;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseReportEntityTypeConfiguration());
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

        await context.SaveChangesAsync(cancellationToken);
    }
}