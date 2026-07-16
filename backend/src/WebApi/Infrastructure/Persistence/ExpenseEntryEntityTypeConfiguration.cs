// Configures EF Core mapping for expense entries.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApi.Domain.ExpenseEntries;
using WebApi.Domain.ExpenseReports;

namespace WebApi.Infrastructure.Persistence;

/// <summary>
/// Maps expense entries, their value objects and soft-deletion fields for PostgreSQL persistence.
/// </summary>
internal sealed class ExpenseEntryEntityTypeConfiguration : IEntityTypeConfiguration<ExpenseEntry>
{
    public void Configure(EntityTypeBuilder<ExpenseEntry> builder)
    {
        builder.ToTable("expense_entries", table =>
        {
            table.HasCheckConstraint("CK_expense_entries_amount_positive", "\"Amount\" > 0");
            table.HasCheckConstraint("CK_expense_entries_currency_eur", "\"Currency\" = 'EUR'");
            table.HasCheckConstraint("CK_expense_entries_report_month_valid", "\"ReportMonth\" BETWEEN 1 AND 12");
            table.HasCheckConstraint(
                "CK_expense_entries_expense_date_report_month",
                "EXTRACT(YEAR FROM \"ExpenseDate\") = \"ReportYear\" AND EXTRACT(MONTH FROM \"ExpenseDate\") = \"ReportMonth\"");
        });
        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.ExpenseReportId).IsRequired();
        builder.Property(entry => entry.ReportYear).IsRequired();
        builder.Property(entry => entry.ReportMonth).IsRequired();
        builder.Property(entry => entry.ExpenseDate).HasColumnType("date").IsRequired();
        builder.Property(entry => entry.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(entry => entry.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(entry => entry.UpdatedAtUtc);
        builder.Property(entry => entry.DeletedAtUtc);

        builder.OwnsOne(entry => entry.Description, description =>
        {
            description.Property(value => value.Value)
                .HasColumnName("Description")
                .HasMaxLength(ExpenseDescription.MaxLength)
                .IsRequired();
        });

        builder.OwnsOne(entry => entry.Amount, amount =>
        {
            amount.Property(value => value.Amount)
                .HasColumnName("Amount")
                .HasPrecision(12, 2)
                .IsRequired();
            amount.OwnsOne(value => value.Currency, currency =>
            {
                currency.Property(value => value.Code)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });
            amount.Navigation(value => value.Currency).IsRequired();
        });

        builder.OwnsOne(entry => entry.BillingAddress, address =>
        {
            address.Property(value => value.MerchantName)
                .HasColumnName("BillingMerchantName")
                .HasMaxLength(200)
                .IsRequired();
            address.Property(value => value.Street)
                .HasColumnName("BillingStreet")
                .HasMaxLength(200)
                .IsRequired();
            address.Property(value => value.PostalCode)
                .HasColumnName("BillingPostalCode")
                .HasMaxLength(20)
                .IsRequired();
            address.Property(value => value.City)
                .HasColumnName("BillingCity")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.HasOne<ExpenseReport>()
            .WithMany()
            .HasForeignKey(entry => new { entry.ExpenseReportId, entry.ReportYear, entry.ReportMonth })
            .HasPrincipalKey(report => new { report.Id, report.Year, report.Month })
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_expense_entries_expense_reports_period");

        builder.HasIndex(entry => entry.ExpenseReportId);
        builder.HasIndex(entry => new { entry.ExpenseReportId, entry.IsDeleted });
        builder.HasIndex(entry => new { entry.ExpenseReportId, entry.IsDeleted, entry.ExpenseDate });
        builder.HasIndex(entry => entry.ExpenseDate);

        builder.Navigation(entry => entry.Description).IsRequired();
        builder.Navigation(entry => entry.Amount).IsRequired();
        builder.Navigation(entry => entry.BillingAddress).IsRequired();
    }
}
