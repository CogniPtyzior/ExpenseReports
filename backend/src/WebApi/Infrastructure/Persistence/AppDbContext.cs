// Defines the EF Core database context used by persistence adapters.
using Microsoft.EntityFrameworkCore;

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
        modelBuilder.ApplyConfiguration(new ExpenseReportEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseEntryEntityTypeConfiguration());
    }
}
