// Defines the EF Core database context and development seed data.
using Microsoft.EntityFrameworkCore;
using WebApi.Domain.Users;

namespace WebApi.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the application persistence adapters.
/// </summary>
internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
    }

    internal static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        if (await context.Set<User>().AnyAsync(cancellationToken))
        {
            return;
        }

        var createdAtUtc = DateTime.UtcNow;
        context.Set<User>().Add(User.Create(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            PersonName.Create("Juste", "Leblanc"),
            PostalAddress.Create("1 Rue des Notes", "75001", "Paris"),
            MonthlyExpenseQuota.Create(5),
            createdAtUtc));
        context.Set<User>().Add(User.Create(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            PersonName.Create("Marc", "Assin"),
            PostalAddress.Create("24 Avenue des Frais", "69002", "Lyon"),
            MonthlyExpenseQuota.Create(8),
            createdAtUtc));

        await context.SaveChangesAsync(cancellationToken);
    }
}