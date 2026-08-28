// Covers expense entry entity and value object invariants.
using WebApi.Core.Exceptions;
using WebApi.Domain.ExpenseEntries;
using WebApi.Domain.ExpenseReports;

namespace WebApi.Tests.Domain.ExpenseEntries;

public sealed class ExpenseEntryTests
{
    private static int CurrentYear => DateTime.UtcNow.Year;
    private static readonly Guid ReportId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly CalendarMonth October2025 = CalendarMonth.Create(CurrentYear, 10);
    private static readonly DateTime Now = new(2025, 10, 15, 8, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Description_is_trimmed_and_accepts_fifty_characters()
    {
        var description = ExpenseDescription.Create($" {new string('a', 50)} ");

        Assert.Equal(new string('a', 50), description.Value);
    }

    [Fact]
    public void Description_rejects_blank_or_too_long_value()
    {
        Assert.Throws<DomainException>(() => ExpenseDescription.Create(" "));
        Assert.Throws<DomainException>(() => ExpenseDescription.Create(new string('a', 51)));
    }

    [Fact]
    public void Billing_address_requires_all_parts_and_trims_values()
    {
        var address = BillingAddress.Create(" Cafe Paris ", " 1 Rue des Notes ", " 75001 ", " Paris ");

        Assert.Equal("Cafe Paris", address.MerchantName);
        Assert.Equal("1 Rue des Notes", address.Street);
        Assert.Equal("75001", address.PostalCode);
        Assert.Equal("Paris", address.City);
        Assert.Throws<DomainException>(() => BillingAddress.Create("", "1 Rue des Notes", "75001", "Paris"));
        Assert.Throws<DomainException>(() => BillingAddress.Create("Cafe Paris", "", "75001", "Paris"));
        Assert.Throws<DomainException>(() => BillingAddress.Create("Cafe Paris", "1 Rue des Notes", "", "Paris"));
        Assert.Throws<DomainException>(() => BillingAddress.Create("Cafe Paris", "1 Rue des Notes", "75001", ""));
    }

    [Fact]
    public void Money_requires_positive_amount_and_eur_currency()
    {
        var money = Money.Create(12.34m, Currency.Create(" eur "));

        Assert.Equal(12.34m, money.Amount);
        Assert.Equal("EUR", money.Currency.Code);
        Assert.Throws<DomainException>(() => Money.Create(0, Currency.Eur));
        Assert.Throws<DomainException>(() => Money.Create(-1, Currency.Eur));
        Assert.Throws<DomainException>(() => Money.Create(1, null!));
        Assert.Throws<DomainException>(() => Currency.Create("USD"));
    }

    [Fact]
    public void Expense_entry_is_created_for_a_date_inside_the_report_month()
    {
        var entry = CreateEntry();

        Assert.Equal(ReportId, entry.ExpenseReportId);
        Assert.Equal(CurrentYear, entry.ReportYear);
        Assert.Equal(10, entry.ReportMonth);
        Assert.Equal(new DateOnly(CurrentYear, 10, 15), entry.ExpenseDate);
        Assert.Equal("Lunch", entry.Description.Value);
        Assert.Equal(25m, entry.Amount.Amount);
        Assert.Equal(Now, entry.CreatedAtUtc);
        Assert.False(entry.IsDeleted);
    }

    [Fact]
    public void Expense_entry_rejects_empty_report_id_and_date_outside_report_month()
    {
        Assert.Throws<DomainException>(() => ExpenseEntry.Create(
            Guid.NewGuid(),
            Guid.Empty,
            October2025,
            new DateOnly(CurrentYear, 10, 15),
            ExpenseDescription.Create("Lunch"),
            Money.Create(25m, Currency.Eur),
            BillingAddress.Create("Cafe Paris", "1 Rue des Notes", "75001", "Paris"),
            Now));
        Assert.Throws<DomainException>(() => ExpenseEntry.Create(
            Guid.NewGuid(),
            ReportId,
            October2025,
            new DateOnly(CurrentYear, 11, 1),
            ExpenseDescription.Create("Lunch"),
            Money.Create(25m, Currency.Eur),
            BillingAddress.Create("Cafe Paris", "1 Rue des Notes", "75001", "Paris"),
            Now));
    }

    [Fact]
    public void Expense_entry_rejects_missing_required_values()
    {
        Assert.Throws<DomainException>(() => ExpenseEntry.Create(
            Guid.NewGuid(),
            ReportId,
            null!,
            new DateOnly(CurrentYear, 10, 15),
            ExpenseDescription.Create("Lunch"),
            Money.Create(25m, Currency.Eur),
            BillingAddress.Create("Cafe Paris", "1 Rue des Notes", "75001", "Paris"),
            Now));
        Assert.Throws<DomainException>(() => ExpenseEntry.Create(
            Guid.NewGuid(),
            ReportId,
            October2025,
            new DateOnly(CurrentYear, 10, 15),
            null!,
            Money.Create(25m, Currency.Eur),
            BillingAddress.Create("Cafe Paris", "1 Rue des Notes", "75001", "Paris"),
            Now));
        Assert.Throws<DomainException>(() => ExpenseEntry.Create(
            Guid.NewGuid(),
            ReportId,
            October2025,
            new DateOnly(CurrentYear, 10, 15),
            ExpenseDescription.Create("Lunch"),
            null!,
            BillingAddress.Create("Cafe Paris", "1 Rue des Notes", "75001", "Paris"),
            Now));
        Assert.Throws<DomainException>(() => ExpenseEntry.Create(
            Guid.NewGuid(),
            ReportId,
            October2025,
            new DateOnly(CurrentYear, 10, 15),
            ExpenseDescription.Create("Lunch"),
            Money.Create(25m, Currency.Eur),
            null!,
            Now));
    }
    [Fact]
    public void Update_replaces_values_when_date_stays_inside_report_month()
    {
        var entry = CreateEntry();

        entry.Update(
            new DateOnly(CurrentYear, 10, 20),
            ExpenseDescription.Create("Taxi"),
            Money.Create(42m, Currency.Eur),
            BillingAddress.Create("Taxi Paris", "2 Rue des Frais", "75002", "Paris"),
            Now.AddHours(1));

        Assert.Equal(new DateOnly(CurrentYear, 10, 20), entry.ExpenseDate);
        Assert.Equal("Taxi", entry.Description.Value);
        Assert.Equal(42m, entry.Amount.Amount);
        Assert.Equal("Taxi Paris", entry.BillingAddress.MerchantName);
        Assert.Equal(Now.AddHours(1), entry.UpdatedAtUtc);
    }

    [Fact]
    public void Update_rejects_missing_required_values()
    {
        var entry = CreateEntry();

        Assert.Throws<DomainException>(() => entry.Update(
            new DateOnly(CurrentYear, 10, 20),
            null!,
            Money.Create(42m, Currency.Eur),
            BillingAddress.Create("Taxi Paris", "2 Rue des Frais", "75002", "Paris"),
            Now.AddHours(1)));
        Assert.Throws<DomainException>(() => entry.Update(
            new DateOnly(CurrentYear, 10, 20),
            ExpenseDescription.Create("Taxi"),
            null!,
            BillingAddress.Create("Taxi Paris", "2 Rue des Frais", "75002", "Paris"),
            Now.AddHours(1)));
        Assert.Throws<DomainException>(() => entry.Update(
            new DateOnly(CurrentYear, 10, 20),
            ExpenseDescription.Create("Taxi"),
            Money.Create(42m, Currency.Eur),
            null!,
            Now.AddHours(1)));
    }
    [Fact]
    public void Update_rejects_date_outside_report_month_without_moving_entry()
    {
        var entry = CreateEntry();

        Assert.Throws<DomainException>(() => entry.Update(
            new DateOnly(CurrentYear, 9, 30),
            ExpenseDescription.Create("Taxi"),
            Money.Create(42m, Currency.Eur),
            BillingAddress.Create("Taxi Paris", "2 Rue des Frais", "75002", "Paris"),
            Now.AddHours(1)));
        Assert.Equal(new DateOnly(CurrentYear, 10, 15), entry.ExpenseDate);
        Assert.Null(entry.UpdatedAtUtc);
    }

    [Fact]
    public void Soft_delete_marks_entry_and_keeps_deleted_timestamp()
    {
        var entry = CreateEntry();

        entry.SoftDelete(Now.AddHours(2));

        Assert.True(entry.IsDeleted);
        Assert.Equal(Now.AddHours(2), entry.DeletedAtUtc);
        Assert.Equal(Now.AddHours(2), entry.UpdatedAtUtc);
    }

    private static ExpenseEntry CreateEntry()
    {
        return ExpenseEntry.Create(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            ReportId,
            October2025,
            new DateOnly(CurrentYear, 10, 15),
            ExpenseDescription.Create("Lunch"),
            Money.Create(25m, Currency.Eur),
            BillingAddress.Create("Cafe Paris", "1 Rue des Notes", "75001", "Paris"),
            Now);
    }
}
