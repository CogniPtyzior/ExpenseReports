// Configures EF Core mapping for the user aggregate.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApi.Domain.Users;

namespace WebApi.Infrastructure.Persistence;

/// <summary>
/// Maps the user aggregate without leaking EF concerns into the domain model.
/// </summary>
internal sealed class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);

        builder.OwnsOne(user => user.Name, name =>
        {
            name.Property(value => value.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();
            name.Property(value => value.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
            name.HasIndex(value => new { value.LastName, value.FirstName });
        });

        builder.OwnsOne(user => user.Address, address =>
        {
            address.Property(value => value.Street)
                .HasColumnName("Street")
                .HasMaxLength(200)
                .IsRequired();
            address.Property(value => value.PostalCode)
                .HasColumnName("PostalCode")
                .HasMaxLength(20)
                .IsRequired();
            address.Property(value => value.City)
                .HasColumnName("City")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Property(user => user.MonthlyExpenseQuota)
            .HasConversion(quota => quota.Value, value => MonthlyExpenseQuota.Create(value))
            .HasColumnName("MonthlyExpenseQuota")
            .HasDefaultValue(MonthlyExpenseQuota.Create(5))
            .IsRequired();

        builder.Property(user => user.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(user => user.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(user => user.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(user => user.UpdatedAtUtc);
        builder.Property(user => user.DeletedAtUtc);

        builder.HasIndex(user => user.IsDeleted);
        builder.HasIndex(user => new { user.IsDeleted, user.IsActive });
    }
}