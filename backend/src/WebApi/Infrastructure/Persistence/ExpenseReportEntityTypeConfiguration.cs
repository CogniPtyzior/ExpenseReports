// Configures EF Core mapping for the expense report aggregate.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApi.Domain.ExpenseReports;
using WebApi.Domain.Users;

namespace WebApi.Infrastructure.Persistence;

/// <summary>
/// Maps expense reports and protects the one-report-per-user-month invariant in the database.
/// </summary>
internal sealed class ExpenseReportEntityTypeConfiguration : IEntityTypeConfiguration<ExpenseReport>
{
    public void Configure(EntityTypeBuilder<ExpenseReport> builder)
    {
        builder.ToTable("expense_reports");
        builder.HasKey(report => report.Id);

        builder.Property(report => report.UserId).IsRequired();
        builder.Property(report => report.AssignedUserFullName).HasMaxLength(201).IsRequired();
        builder.Property(report => report.Year).IsRequired();
        builder.Property(report => report.Month).IsRequired();
        builder.Property(report => report.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(report => report.UpdatedAtUtc);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(report => report.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(report => report.UserId);
        builder.HasIndex(report => new { report.Year, report.Month });
        builder.HasIndex(report => new { report.UserId, report.Year, report.Month }).IsUnique();
    }
}